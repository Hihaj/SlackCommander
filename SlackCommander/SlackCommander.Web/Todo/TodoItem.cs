﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlackCommander.Web.Todo
{
    [Serializable]
    public class TodoItem
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool Done { get; set; }
    }
}