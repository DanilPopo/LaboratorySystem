using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaboratorySystem
{
    public class Service1
    {
        public int id { get; set; }
        public string service { get; set; }
        public decimal price { get; set; }
        public int lead_time { get; set; }
        public double deviation_from { get; set; }
        public double deviation_to { get; set; }
        public bool tr { get; set; }
        public Service1(int id, string service, decimal price, int lead_time, double deviation_from, double deviation_to, bool tr)
        {
            this.id = id;
            this.service = service;
            this.price = price;
            this.lead_time = lead_time;
            this.deviation_from = deviation_from;
            this.deviation_to = deviation_to;
            this.tr = tr;
        }
    }
}
