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

public class SubscriberLogic : BaseNetLogic
{
    public override void Start()
    {
        try
        {
            var brokerIpAddressVariable = Project.Current.GetVariable("Model/BrokerIpAddress");

            // Create a client connecting to the broker (default port is 1883)
            subscribeClient = new MqttClient(brokerIpAddressVariable.Value);
            // Connect to the broker
            subscribeClient.Connect("FTOptixSubscribeClient");
            // Assign a callback to be executed when a message is received from the broker
            subscribeClient.MqttMsgPublishReceived += SubscribeClientMqttMsgPublishReceived;
            // Subscribe to the "my_topic" topic with QoS 2
            ushort msgId = subscribeClient.Subscribe(new string[] { "/my_topic" }, // topic
                new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE }); // QoS level

            messageVariable = Project.Current.GetVariable("Model/Message");
        }
        catch
        {
            Log.Error("PublisherLogic", "Cannot connect to MQTT server!");
        }
    }

    public override void Stop()
    {
        subscribeClient.Unsubscribe(new string[] { "/my_topic" });
        subscribeClient.Disconnect();
    }

    private void SubscribeClientMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        messageVariable.Value = "Message received: " + System.Text.Encoding.UTF8.GetString(e.Message);
    }

    private MqttClient subscribeClient;
    private IUAVariable messageVariable;
}
