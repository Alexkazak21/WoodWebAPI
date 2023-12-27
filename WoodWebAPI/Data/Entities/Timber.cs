using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WoodWebAPI.Data.Entities;

public partial class Timber
{
    public int Id { get; set; }

    public double Length { get; set; }

    public int Diameter { get; set; }

    public double Volume { get; set; }

}
