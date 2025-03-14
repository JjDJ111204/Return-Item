using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Return_Item
{
    class User
    {
        public int Borrowed_ID { get; set; }
        public string Borrower_Name { get; set; }
        public int Item_ID { get; set; }
        public string Item_Name { get; set; }
        public string Item_Description { get; set; }
        public int Borrowed_Quantity { get; set; }
        public int Item_Low_Indicator { get; set; }
        public int Category_ID { get; set; }
        public DateTime Borrow_Transaction_Date { get; set; }
        public int Activity_ID { get; set; }

    }
}
