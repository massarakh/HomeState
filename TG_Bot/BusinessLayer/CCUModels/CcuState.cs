namespace TG_Bot.BusinessLayer.CCUModels
{
    public class CcuState
    {
        public Input[] Inputs { get; set; }
        public int[] Outputs { get; set; }

        public int[] Relays
        {
            get
            {
                return new int[2] { Outputs[0], Outputs[1] };
            }
        }

        public string[] Partitions { get; set; }
        public int Case { get; set; }
        public float Power { get; set; }
        public Battery Battery { get; set; }
        public int Temp { get; set; }
        public string Balance { get; set; }
        public Event[] Events { get; set; }
    }
}
