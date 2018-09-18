namespace Consumer.Messages
{
    public class ReceiveCommands
    {
        public ReceiveCommands()
        {
            NumberOfCommands = 10;
        }

        public int NumberOfCommands { get; set; }
    }
}