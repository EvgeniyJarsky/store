﻿namespace Store.Web.Models
{
    public class ConfirmationModel
    {

        public int OrderId { get; set; }

        public string CellPhone { get; set; }

        public Dictionary<string, string> Error { get; set; } = new Dictionary<string, string>();

    }
}
