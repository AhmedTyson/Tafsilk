using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AppSetting
{
    public Guid Id { get; set; }
<<<<<<< Updated upstream
    public string Key { get; set; }
    public string Value { get; set; }
=======
    public required string Key { get; set; }
    public required string Value { get; set; }
>>>>>>> Stashed changes
    public DateTime LastUpdated { get; set; }
}

