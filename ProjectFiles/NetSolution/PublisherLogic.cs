#region StandardUsing
using System;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.OPCUAServer;
#endregion
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class PublisherLogic : BaseNetLogic
{
    public override void Start()
    {
        try
        {
            var brokerIpAddressVariable = Project.Current.GetVariable("Model/BrokerIpAddress");

            // Create a client connecting to the broker (default port is 1883)
            publishClient = new MqttClient(brokerIpAddressVariable.Value);
            // Connect to the broker
            publishClient.Connect("FTOptixPublishClient");
            // Assign a callback to be executed when a message is published to the broker
            publishClient.MqttMsgPublished += PublishClientMqttMsgPublished;
        }
        catch
        {
            Log.Error("PublisherLogic", "Cannot connect to MQTT server!");
        }
    }

    public override void Stop()
    {
        publishClient.Disconnect();
        publishClient.MqttMsgPublished -= PublishClientMqttMsgPublished;
    }

    private void PublishClientMqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
    {
        Log.Info("Message " + e.MessageId + " - published = " + e.IsPublished);
    }

    [ExportMethod]
    public void PublishMessage()
    {
        var variable1 = Project.Current.GetVariable("Model/Variable1");
        variable1.Value = new Random().Next(0, 101);

        // Publish a message
        ushort msgId = publishClient.Publish("/my_topic", // topic
            System.Text.Encoding.UTF8.GetBytes(((int)variable1.Value).ToString()), // message body
            MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, // QoS level
            false); // retained
    }

    private MqttClient publishClient;
}
