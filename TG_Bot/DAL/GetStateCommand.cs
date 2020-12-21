using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TG_Bot.monitoring;

namespace TG_Bot.DAL
{
    public class GetStateCommand : IRequest<Monitor>
    {
        public Monitor State { get; set; }
    }
}
