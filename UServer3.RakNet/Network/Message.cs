namespace RakNet.Network
{
    public class Message
    {
        public Message.Type type;

        public NetworkPeer peer;

        public Connection connection;

        public Read read => this.peer.read;

        public Write write => this.peer.write;

        public Message()
        {
        }

        public virtual void Clear()
        {
            this.connection = null;
            this.peer = null;
            this.type = (Message.Type)0;
        }

        public enum Type : byte
        {
            Welcome = 1,
            Auth,
            Approved,
            Ready,
            Entities,
            EntityDestroy,
            GroupChange,
            GroupDestroy,
            RPCMessage,
            EntityPosition,
            ConsoleMessage,
            ConsoleCommand,
            Effect,
            DisconnectReason,
            Tick,
            Message,
            RequestUserInformation,
            GiveUserInformation,
            GroupEnter,
            GroupLeave,
            VoiceData,
            EAC,
            EntityFlags,
            World,
            ConsoleReplicatedVars,
            Last = 25
        }
    }
}