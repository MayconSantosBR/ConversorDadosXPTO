﻿using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversorDadosXPTO.Models
{
    public class GarantiaSafra
    {
        [Index(0)]
        public string Date { get; set; }

        [Index(1)]
        public string StateAcronym { get; set; }

        [Index(2)]
        public int CityCode { get; set; }

        [Index(3)]
        public string CityName { get; set; }

        [Index(4)]
        public string Nis { get; set; }

        [Index(5)]
        public string Person { get; set; }

        [Index(6)]
        public double InstallmentValue { get; set; }
    }
}