﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public interface IValidateService
    {
        bool ValidateName(string name);
        bool ValidatePhone(string phone);
    }
}