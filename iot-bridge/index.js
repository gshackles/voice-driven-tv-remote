const config = require('./config.json');
const request = require('request-promise');
const tedious = require('tedious');
const broker = require('aedes')();
const Azure = require('azure-iot-device');
const Mqtt = require('azure-iot-device-mqtt').Mqtt;

const mqttServer = require('net').createServer(broker.handle);
mqttServer.listen(1883, () => console.log('MQTT broker listening'));

function updateAvailableCommands(commands) {
    const connection = new tedious.Connection(config.sqlConfig);
    connection.on('connect', err => {
        if (err) {
            console.error('Error connecting to SQL', err);
            return;
        }

        const truncateRequest = new tedious.Request("truncate table AvailableCommand", err => {
            if (err) {
                console.error('Error truncating table', err);
            }
        });

        truncateRequest.on('requestCompleted', () => {
            const updateCommands = connection.newBulkLoad('AvailableCommand', (err, rowCount) => {
                if (err) {
                    console.error('Error inserting commands', err);
                } else {
                    console.log(`Inserted ${rowCount} command(s)`);
                }
            });
            updateCommands.addColumn('Name', tedious.TYPES.NVarChar, { nullable: false });
            updateCommands.addColumn('Slug', tedious.TYPES.NVarChar, { nullable: false });
            updateCommands.addColumn('Label', tedious.TYPES.NVarChar, { nullable: false });

            commands.forEach(command => 
                updateCommands.addRow({ Name: command.name, 
                                        Slug: command.slug, 
                                        Label: command.label }));

            connection.execBulkLoad(updateCommands);
        })

        connection.execSql(truncateRequest);
    }); 
}

broker.on('publish', packet => {
    if (packet.topic !== 'harmony-api/hubs/living-room/current_activity') {
        return;
    }

    request({ url: 'http://localhost:8282/hubs/living-room/commands', json: true })
        .then(res => updateAvailableCommands(res.commands));
});

const azureClient = Azure.Client.fromConnectionString(config.iotHubConnectionString, Mqtt);

function onAzureConnect(err) {
    if (err) {
        console.error('Could not connect to Azure', err);
        return;
    }

    console.log('Connected to Azure');

    azureClient.on('error', console.error);
    azureClient.on('disconnect', function () {
        azureClient.removeAllListeners();
        azureClient.open(onAzureConnect);
    });

    azureClient.on('message', msg => {
        const [topic, payload] = msg.data.toString().split(';');
        console.info(`received ${topic}: ${payload}`);

        broker.publish({
            topic,
            payload
        });
    });
}

azureClient.open(onAzureConnect);