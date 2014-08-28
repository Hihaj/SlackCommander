﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackCommander.Web
{
    public interface ICommandHandler
    {
        Task<dynamic> Handle(Command command);
    }
}
