﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TG_Bot.monitoring;

namespace TG_Bot.BusinessLayer
{
    public interface IStateService
    {
        Task<Monitor> LastState();
    }
}
