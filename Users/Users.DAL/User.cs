using System;
using System.Collections.Generic;
using System.Text;

namespace Users.DAL
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }    // AES256 en BD/DAL
        public string TaxId { get; set; }       // RFC
        public string CreatedAt { get; set; }   // dd-MM-yyyy HH:mm Madagascar
        public List<Address> Addresses { get; set; }
    }
}
