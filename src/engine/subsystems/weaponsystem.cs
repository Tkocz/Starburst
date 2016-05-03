﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fab5.Engine.Core;
using Fab5.Engine.Components;

// DEN HÄR SKA INTE VARA HÄR WTF
using Fab5.Starburst.States.Playing.Entities;

namespace Fab5.Engine.Subsystems {
    public class Weapon_System : Subsystem {
        Game_State gameState;
        public Weapon_System(Game_State state) {
            this.gameState = state;
        }
        public override void init() {
            // ladda in texturer för skott
            base.init();
        }
        public override void on_message(string msg, dynamic data) {
            if(msg.Equals("fireInput")) {
                Entity origin = data.Origin;
                Weapon weapon = data.Weapon;
                Ship_Info ship = data.Ship;

                Bullet_Factory.fire_weapon(origin, weapon);
                if (!ship.has_powerup("free-fire")) {
                    ship.energy_value -= weapon.energy_cost;
                }
                Fab5_Game.inst().message("fire", weapon);
                weapon.timeSinceLastShot = 0f;
            }
        }
        public override void update(float t, float dt) {
            var entities = Fab5_Game.inst().get_entities_fast( typeof(Primary_Weapon));
            int numberOfWeapons = entities.Count;

            for (int i = 0; i < numberOfWeapons; i++) {
                Weapon weapon = entities[i].get_component<Primary_Weapon>();
                Weapon weapon2 = entities[i].get_component<Secondary_Weapon>();
                if(weapon != null && weapon.timeSinceLastShot <= weapon.fire_rate)
                    weapon.timeSinceLastShot += dt;
                if (weapon2 != null && weapon2.timeSinceLastShot <= weapon2.fire_rate)
                    weapon2.timeSinceLastShot += dt;
            }

            base.update(t, dt);
        }
    }
}
