﻿using Expressif.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressif.Testing.Values
{
    public class WeekdaysTest
    {
        [Test]
        public void Monday_Index_IsFirstDay()
            => Assert.That(Weekdays.Monday.Index, Is.EqualTo(0));
        [Test]
        public void Monday_Name_Monday()
            => Assert.That(Weekdays.Monday.Name, Is.EqualTo("Monday"));
        [Test]
        public void Sunday_Index_IsLastDay()
            => Assert.That(Weekdays.Sunday.Index, Is.EqualTo(6));
        [Test]
        public void Sunday_Name_Sunday()
            => Assert.That(Weekdays.Sunday.Name, Is.EqualTo("Sunday"));
        [Test]
        public void AllDays_Index_CorrectlyOrdered()
            => Assert.That(
                    Weekdays.Monday.Index < Weekdays.Tuesday.Index &&
                    Weekdays.Tuesday.Index < Weekdays.Wednesday.Index &&
                    Weekdays.Wednesday.Index < Weekdays.Thursday.Index &&
                    Weekdays.Thursday.Index  < Weekdays.Friday.Index &&
                    Weekdays.Friday.Index < Weekdays.Saturday.Index &&
                    Weekdays.Saturday.Index < Weekdays.Sunday.Index
                );
    }
}
