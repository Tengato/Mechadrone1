﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Mechadrone1.Rendering
{
    interface ICamera
    {
        Matrix View { get; }
        Matrix Transform { get; }
    }
}