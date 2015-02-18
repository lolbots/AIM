﻿#region LICENSE

// Copyright 2014 - 2015 LeagueSharp
// Default.cs is part of AiM.
//
// AiM is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// AiM is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with AiM. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using AIM.Util;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace AIM.Plugins
{
    public class Default : PluginBase
    {
        public Default()
        {
            Author = "imsosharp";
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 200);
            R = new Spell(SpellSlot.R, 600);

            var q = SpellData.GetSpellData(ObjectHandler.Player.GetSpell(SpellSlot.Q).Name);
            var w = SpellData.GetSpellData(ObjectHandler.Player.GetSpell(SpellSlot.W).Name);
            var e = SpellData.GetSpellData(ObjectHandler.Player.GetSpell(SpellSlot.E).Name);
            var r = SpellData.GetSpellData(ObjectHandler.Player.GetSpell(SpellSlot.R).Name);

            Q.SetSkillshot(q.SpellCastTime, q.LineWidth, q.MissileSpeed, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(w.SpellCastTime, w.LineWidth, w.MissileSpeed, true, SkillshotType.SkillshotLine);
            E.SetTargetted(e.SpellCastTime, e.SpellCastTime);
            R.SetTargetted(r.SpellCastTime, r.SpellCastTime);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                if (Q.CastCheck(Target, "ComboQ"))
                {
                    Q.Cast(Target);
                }
                if (W.CastCheck(Target, "ComboW"))
                {
                    W.Cast(Target);
                }
                if (E.CastCheck(Target, "ComboE"))
                {
                    E.Cast(Target);
                }

                if (R.CastCheck(Target, "ComboR"))
                {
                    R.Cast(Target);
                }
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboE", "Use E", true);
            config.AddBool("ComboR", "Use R", true);
        }
    }
}