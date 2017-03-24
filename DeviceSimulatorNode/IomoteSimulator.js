'use strict';

 var Mqtt = require('azure-iot-device-mqtt').Mqtt;
 var DeviceClient = require('azure-iot-device').Client;

 var connectionString = 'HostName=iomoteHub01.azure-devices.net;DeviceId=VJHackfestDemo;SharedAccessKey=rReAqYjrLxC2BDXT2ZY32DBxmvKfxBoVYB8Ja6I6dKM=';
 var client = DeviceClient.fromConnectionString(connectionString, Mqtt);

function onMeasureTemperature(request, response) {
     console.log(request.payload);

     response.send(200, '20', function(err) {
         if(err) {
             console.error('An error ocurred when sending a method response:\n' + err.toString());
         } else {
             console.log('Response to method \'' + request.methodName + '\' sent successfully.' );
         }
     });
 }

 client.open(function(err) {
     if (err) {
         console.error('could not open IotHub client');
     }  else {
         console.log('client opened');
         client.onDeviceMethod('MeasureTemperature', onMeasureTemperature);
     }
 });