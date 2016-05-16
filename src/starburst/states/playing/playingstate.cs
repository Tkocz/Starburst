namespace Fab5.Starburst.States {

using Fab5.Engine;
using Fab5.Engine.Components;
using Fab5.Engine.Core;
using Fab5.Engine.Subsystems;

using Fab5.Starburst.States.Playing;
using Fab5.Starburst.States.Playing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using System.Collections.Generic;

using System;

public class Playing_State : Game_State {

    bool can_pause = false;
    public static System.Random rand = new System.Random();
    private Collision_Handler coll_handler;
    private List<Input> inputs;
    public Spawn_Util spawner;

    public Playing_State(List<Input> inputs, Game_Config conf = null) {
        this.inputs = inputs;

        this.game_conf = conf ?? new Game_Config();
        this.spawner = new Spawn_Util(game_conf);
    }

    public readonly Game_Config game_conf = new Game_Config();

    public override void on_message(string msg, dynamic data) {
        if (msg == "collision") {
            coll_handler.on_collision(data.entity1, data.entity2, data);
            return;
        }
        else if (msg.Equals("start")) {
            tryPause();
        }
        else if(msg.Equals("fullscreen")) {
            Starburst.inst().GraphicsMgr.ToggleFullScreen();
            System.Threading.Thread.Sleep(150);
        }
    }
        private void tryPause() {
            if (can_pause) {
                can_pause = false;
                Starburst.inst().enter_state(new Pause_State(renderer.backbuffer_target, this));
            }
        }

    public Tile_Map tile_map;

    private void load_map(System.Drawing.Bitmap bitmap, int[] tiles) {
        for (int x = 0; x < 256; x++) {
            for (int y = 0; y < 256; y++) {
                int i = x+y*256;

                tiles[i] = 0;

                var c = bitmap.GetPixel(x, y);

                if (c == System.Drawing.Color.FromArgb(127, 127, 127)) {
                    tiles[i] = 1;
                }
                if (c == System.Drawing.Color.FromArgb(0, 0, 0)) {
                    tiles[i] = 2;
                }
                else if (c == System.Drawing.Color.FromArgb(0, 255, 0)) {
                    tiles[i] = 3;
                }
                else if (c == System.Drawing.Color.FromArgb(255, 0, 0)) {
                    tiles[i] = 4;
                }
                else if (c == System.Drawing.Color.FromArgb(255, 255, 0)) {
                    tiles[i] = 5;
                }
                else if (c == System.Drawing.Color.FromArgb(127, 0, 0)) {
                    tiles[i] = 6;
                }
                else if (c == System.Drawing.Color.FromArgb(127, 63, 127)) {
                    // pepita
                    tiles[i] = 7;
                }
                else if (c == System.Drawing.Color.FromArgb(127, 127, 0)) {
                    // soccer net team 1 (team 2 scores here)
                    tiles[i] = 8;
                }
                else if (c == System.Drawing.Color.FromArgb(127, 63, 0)) {
                    // soccer net team 2 (team 1 scores here)
                    tiles[i] = 9;
                }
                else if (c == System.Drawing.Color.FromArgb(255, 0, 255)) {
                    // soccer spawn
                    tiles[i] = 10;
                }
                else if (c == System.Drawing.Color.FromArgb(0, 255, 255)) {
                    // powerup spawn
                    tiles[i] = 11;
                }
                else if (c == System.Drawing.Color.FromArgb(255, 127, 0)) {
                    // team 1 spawn
                    tiles[i] = 12;
                }
                else if (c == System.Drawing.Color.FromArgb(0, 127, 255)) {
                    // team 2 spawn
                    tiles[i] = 13;
                }
                else if (c == System.Drawing.Color.FromArgb(127, 0, 255)) {
                    // asteroid spawn
                    tiles[i] = 14;
                }
                else if (c.G == 63 && c.B == 63) {
                    var types = new Type[] {
                        typeof (Red_Fountain),
                        typeof (Blue_Fountain),
                        typeof (Red_Turret),
                        typeof (Blue_Turret),
                        typeof (Lamp),
                        typeof (Red_Lamp),
                        typeof (Blue_Lamp),
                    };

                    int k = c.R % types.Length;

                    var type = types[k];
                    var factory = type.GetMethod("create_components", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    var p = create_entity((Component[])factory.Invoke(null, null)).get_component<Position>();
                    if (p != null) {
                        p.x = -2048.0f + x*16.0f+8.0f;
                        p.y = -2048.0f + y*16.0f+8.0f;
                        Console.WriteLine("spawned {0} @ {1}, {2}", type.Name, p.x, p.y);
                    }
                    else {
                        Console.WriteLine("spawned {0}", type.Name);
                    }

                }
            }
        }
    }

    private Rendering_System renderer;

    public override void init() {
        Player_Ship.lol = 1; // @To-do: lol

        MediaPlayer.Volume = game_conf.music_vol;
//        Starburst.inst().IsMouseVisible = true;        // @To-do: Load map here.

        tile_map = new Tile_Map();
        coll_handler = new Collision_Handler(this, tile_map, spawner);

//        game_conf.powerup_spawn_time = 1.0f;
//        game_conf.num_powerups = 50;

        var map_name = System.IO.Path.GetFileNameWithoutExtension(game_conf.map_name);
        var s = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), map_name + ".png");
        using (var bitmap = new System.Drawing.Bitmap(s)) {
            load_map(bitmap, tile_map.tiles);
        }
        s = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), map_name + "_bg.png");
        using (var bitmap = new System.Drawing.Bitmap(s)) {
            load_map(bitmap, tile_map.bg_tiles);
        }

        add_subsystems(
            new /*Async_*/Multi_Subsystem(
                new Multi_Subsystem(
                    new Position_Integrator(),
                    new Collision_Solver(tile_map)
                ),
                new /*Async_*/Multi_Subsystem(
                    new Inputhandler_System(),
                    new Sound(),
                    new Particle_System(),
                    new Lifetime_Manager(),
                    new Weapon_System(this),
                    new AI()
                ),
                new Multi_Subsystem(
                    renderer = new Rendering_System(Starburst.inst().GraphicsDevice) {
                        tile_map = tile_map,
                        match_time = game_conf.match_time
                    },
                    new Window_Title_Writer()
               )
            )
        );

        create_entity(new FpsCounter());

        for(int i = 0; i < inputs.Count; i++) {
            if (inputs[i] == null)
                continue;
            var player = create_entity(Player_Ship.create_components(inputs[i], game_conf, i < 2 ? 1 : 2));
            var player_spawn = spawner.get_player_spawn_pos(player, tile_map);
            player.get_component<Position>().x = player_spawn.x;
            player.get_component<Position>().y = player_spawn.y;
            player.get_component<Angle>().angle = (float)rand.NextDouble() * 6.28f;

            var player_pos = player.get_component<Position>();

            for (int j = 0; j < 20; j++) {
                Fab5_Game.inst().create_entity(new Component[] {
                    new TTL {
                        max_time = j*0.05f,
                        destroy_cb = () => {
                            var theta1 = 2.0f*3.1415f*(float)rand.NextDouble();
                            var theta2 = 2.0f*3.1415f*(float)rand.NextDouble();
                            var radius = 13.0f * (float)rand.NextDouble();
                            var speed  = (30.0f + 110.0f * (float)Math.Pow(rand.NextDouble(), 2.0f));

                            Fab5_Game.inst().create_entity(new Component[] {
                                    new Sprite { blend_mode = Sprite.BM_ADD,
                                                 scale      = 0.8f + (float)rand.NextDouble(),
                                                 texture    = Fab5_Game.inst().get_content<Texture2D>("particle") },

                                    new Position { x = player_pos.x + (float)Math.Cos(theta1) * radius,
                                                   y = player_pos.y + (float)Math.Sin(theta1) * radius },

                                    new Velocity { x = (float)Math.Cos(theta2) * speed,
                                                   y = (float)Math.Sin(theta2) * speed },

                                    new Mass { drag_coeff = 2.0f },

                                    new TTL { alpha_fn = (x, max) => 1.0f - (x*x)/(max*max),
                                              max_time = 1.5f + (float)rand.NextDouble() }
                                });
                        }
                    }
                });
            }

            /*if (i == 0) {
                player.get_component<Position>().x = -1800;
                player.get_component<Position>().y = 1500;
                player.get_component<Velocity>().x = 55.0f;
                player.get_component<Velocity>().y = 0.0f;
            }*/
        }

        create_entity(SoundManager.create_backmusic_component());
        create_entity(SoundManager.create_soundeffects_component());

        for (int i = 0; i < game_conf.num_asteroids; i++) {
            var asteroid = create_entity(Asteroid.create_components());
            var r = asteroid.get_component<Bounding_Circle>().radius;

            Position ap;

            int num_fails = 0;
            bool colliding = false;
            do {
                colliding = false;
                ap = asteroid.get_component<Position>();
                var sp = spawner.get_asteroid_spawn_pos(tile_map);
                ap.x = sp.x;
                ap.y = sp.y;

                foreach (var ast in Starburst.inst().get_entities_fast(typeof (Bounding_Circle))) {
                    if (ast == asteroid) {
                        continue;
                    }

                    var dx = ast.get_component<Position>().x - asteroid.get_component<Position>().x;
                    var dy = ast.get_component<Position>().y - asteroid.get_component<Position>().y;

                    var dist = (dx*dx+dy*dy);

                    var min_dist = ast.get_component<Bounding_Circle>().radius + asteroid.get_component<Bounding_Circle>().radius;
                    min_dist *= 1.05f;
                    min_dist *= min_dist;

                    if (dist < min_dist) {
                        colliding = true;
                        num_fails++;
                        break;
                    }
                }

                if (num_fails > 1000) {
                    // failed to spawn this one.
                    asteroid.destroy();
                    break;
                }
            } while (colliding);

            var av = asteroid.get_component<Velocity>();

            av.x = -15 + 30 * (float)rand.NextDouble();
            av.y = -15 + 30 * (float)rand.NextDouble();
        }

        if (game_conf.enable_soccer) {
            var ball = create_entity(Soccer_Ball.create_components());
            var ball_pos = spawner.get_soccerball_spawn_pos(tile_map);
            ball.get_component<Position>().x = ball_pos.x;
            ball.get_component<Position>().y = ball_pos.y;
            ball.get_component<Angle>().ang_vel = 3.141592f * 2.0f * -2.0f;
        }

        //create_entity(Multifire_Powerup.create_components());
        //create_entity(Turbo_Powerup.create_components());


        /*var shield1 = create_entity(Powerup.create(new Fast_Bombs_Powerup()));
        shield1.get_component<Position>().x = -1800.0f; shield1.get_component<Position>().y = 1500.0f;

        var multi1 = create_entity(Powerup.create(new Free_Fire_Powerup()));
        multi1.get_component<Position>().x = -1700.0f; multi1.get_component<Position>().y = 1500.0f;

        var nano1 = create_entity(Powerup.create(new Bouncy_Bullets_Powerup()));
        nano1.get_component<Position>().x = -1600.0f; multi1.get_component<Position>().y = 1500.0f;*/

        /*var freefire1 = create_entity(Powerup.create(new Free_Fire_Powerup()));
        freefire1.get_component<Position>().x = -1600.0f; freefire1.get_component<Position>().y = 1500.0f;

        var turbo1 = create_entity(Powerup.create(new Turbo_Powerup()));
        turbo1.get_component<Position>().x = -1500.0f; turbo1.get_component<Position>().y = 1500.0f;

        var shield2 = create_entity(Powerup.create(new Shield_Powerup()));
        shield2.get_component<Position>().x = 1800.0f; shield2.get_component<Position>().y = -1500.0f;

        var multi2 = create_entity(Powerup.create(new Multifire_Powerup()));
        multi2.get_component<Position>().x = 1700.0f; multi2.get_component<Position>().y = -1500.0f;

        var freefire2 = create_entity(Powerup.create(new Free_Fire_Powerup()));
        freefire2.get_component<Position>().x = 1600.0f; freefire2.get_component<Position>().y = -1500.0f;

        var turbo2 = create_entity(Powerup.create(new Turbo_Powerup()));
        turbo2.get_component<Position>().x = 1500.0f; turbo2.get_component<Position>().y = -1500.0f;*/

        Dummy_Enemy.ai_index = 1;

        for(int i = 0; i < game_conf.red_bots; i++) {
            var ai = Starburst.inst().create_entity(Dummy_Enemy.create_components(game_conf, 1 /* red team */));
            var aisi = ai.get_component<Ship_Info>();
            if (game_conf.mode == Game_Config.GM_TEAM_DEATHMATCH) aisi.team = 1;
            Console.WriteLine("created ai for team {0}", aisi.team);
            ai.get_component<Bounding_Circle>().ignore_collisions2 = aisi.team;
            var ai_spawn = spawner.get_player_spawn_pos(ai, tile_map);
            ai.get_component<Position>().x = ai_spawn.x;
            ai.get_component<Position>().y = ai_spawn.y;
            ai.get_component<Angle>().angle = (float)rand.NextDouble() * 6.28f;

            var player_pos = ai.get_component<Position>();

            for (int j = 0; j < 20; j++) {
                Fab5_Game.inst().create_entity(new Component[] {
                    new TTL {
                        max_time = j*0.05f,
                        destroy_cb = () => {
                            var theta1 = 2.0f*3.1415f*(float)rand.NextDouble();
                            var theta2 = 2.0f*3.1415f*(float)rand.NextDouble();
                            var radius = 13.0f * (float)rand.NextDouble();
                            var speed  = (30.0f + 110.0f * (float)Math.Pow(rand.NextDouble(), 2.0f));

                            Fab5_Game.inst().create_entity(new Component[] {
                                    new Sprite { blend_mode = Sprite.BM_ADD,
                                                 scale      = 0.8f + (float)rand.NextDouble(),
                                                 texture    = Fab5_Game.inst().get_content<Texture2D>("particle") },

                                    new Position { x = player_pos.x + (float)Math.Cos(theta1) * radius,
                                                   y = player_pos.y + (float)Math.Sin(theta1) * radius },

                                    new Velocity { x = (float)Math.Cos(theta2) * speed,
                                                   y = (float)Math.Sin(theta2) * speed },

                                    new Mass { drag_coeff = 2.0f },

                                    new TTL { alpha_fn = (x, max) => 1.0f - (x*x)/(max*max),
                                              max_time = 1.5f + (float)rand.NextDouble() }
                                });
                        }
                    }
                });
            }
        }

        for(int i = 0; i < game_conf.blue_bots; i++) {
            var ai = Starburst.inst().create_entity(Dummy_Enemy.create_components(game_conf, 2 /* blue team */));
            var aisi = ai.get_component<Ship_Info>();
            if (game_conf.mode == Game_Config.GM_TEAM_DEATHMATCH) aisi.team = 2;
            Console.WriteLine("created ai for team {0}", aisi.team);
            ai.get_component<Bounding_Circle>().ignore_collisions2 = aisi.team;
            var ai_spawn = spawner.get_player_spawn_pos(ai, tile_map);
            ai.get_component<Position>().x = ai_spawn.x;
            ai.get_component<Position>().y = ai_spawn.y;
            ai.get_component<Angle>().angle = (float)rand.NextDouble() * 6.28f;

            for (int j = 0; j < 20; j++) {
                Fab5_Game.inst().create_entity(new Component[] {
                    new TTL {
                        max_time = j*0.05f,
                        destroy_cb = () => {
                            var theta1 = 2.0f*3.1415f*(float)rand.NextDouble();
                            var theta2 = 2.0f*3.1415f*(float)rand.NextDouble();
                            var radius = 13.0f * (float)rand.NextDouble();
                            var speed  = (30.0f + 110.0f * (float)Math.Pow(rand.NextDouble(), 2.0f));

                            Fab5_Game.inst().create_entity(new Component[] {
                                    new Sprite { blend_mode = Sprite.BM_ADD,
                                                 scale      = 0.8f + (float)rand.NextDouble(),
                                                 texture    = Fab5_Game.inst().get_content<Texture2D>("particle") },

                                    new Position { x = ai_spawn.x + (float)Math.Cos(theta1) * radius,
                                                   y = ai_spawn.y + (float)Math.Sin(theta1) * radius },

                                    new Velocity { x = (float)Math.Cos(theta2) * speed,
                                                   y = (float)Math.Sin(theta2) * speed },

                                    new Mass { drag_coeff = 2.0f },

                                    new TTL { alpha_fn = (x, max) => 1.0f - (x*x)/(max*max),
                                              max_time = 1.5f + (float)rand.NextDouble() }
                                });
                        }
                    }
                });
            }
        }

        //var nme = create_entity(Dummy_Enemy.create_components());
        //nme.get_component<Position>().x = -1800.0f;
        //nme.get_component<Position>().y = 1800.0f;

        Starburst.inst().message("play_sound_asset", new { name = "begin_game" });

        create_entity(new Component[] {
            new TTL { max_time = game_conf.match_time,
                      destroy_cb = () => {
                          Playing_State gameState = this;
                          var scoreEntities = gameState.get_entities_fast(typeof(Score));
                          List<Entity> players = new List<Entity>();
                          for(int i=0; i < scoreEntities.Count; i++) {
                              if((scoreEntities[i].get_component<Ship_Info>() != null) && (scoreEntities[i].get_component<Velocity>() != null))
                                  players.Add(scoreEntities[i]);
                          }
                          Starburst.inst().leave_state();
                          Starburst.inst().leave_state();
                          Starburst.inst().enter_state(new Results_State(players, gameState.game_conf));
                      }
            }
        });
    }

    private Entity new_random_powerup() {
        var types = new Type[] {
            typeof (Turbo_Powerup),
            typeof (Free_Fire_Powerup),
            typeof (Shield_Powerup),
            typeof (Multifire_Powerup),
            typeof (Bouncy_Bullets_Powerup),
            typeof (Fast_Bombs_Powerup)
            //typeof (Nanobots_Powerup)
        };

        var i = rand.Next(0, types.Length);

        if (game_conf.soccer_mode) {
            i = 0; // only turbo in soccer mode
        }

        object impl = Activator.CreateInstance(types[i]);

        return create_entity(Powerup.create((Powerup_Impl)impl));
    }

    private float powerup_spawn_timer;
    public override void draw(float t, float dt) {
        base.draw(t, dt);

        var ships = Starburst.inst().get_entities_fast(typeof(Ship_Info));
        for(int i=0; i < ships.Count; i++)
        {
            Ship_Info ship = ships[i].get_component<Ship_Info>();
            if (ship.energy_value < ship.top_energy)
                ship.energy_value += ship.recharge_rate * dt;
            else if (ship.energy_value > ship.top_energy)
                ship.energy_value = ship.top_energy;

        }

        powerup_spawn_timer -= dt;
        if (powerup_spawn_timer <= 0.0f) {
            powerup_spawn_timer = game_conf.powerup_spawn_time;

            int num_powerups_now = Starburst.inst().get_entities_fast(typeof (Powerup)).Count;
            if (num_powerups_now < game_conf.num_powerups) {
                var powerup = new_random_powerup();
                var powerup_pos = spawner.get_powerup_spawn_pos(tile_map);
                powerup.get_component<Position>().x = powerup_pos.x;
                powerup.get_component<Position>().y = powerup_pos.y;
            }
        }

        if (!can_pause) {
                can_pause = true;
                /*
            bool no_buttons_pressed = true;

            for (int i = 0; i <= 3; i++) {
                if (GamePad.GetState((PlayerIndex)i).IsConnected && GamePad.GetState((PlayerIndex)i).Buttons.Start == ButtonState.Pressed) {
                    no_buttons_pressed = false;
                    break;
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.P)) {
                no_buttons_pressed = false;
            }

            if (no_buttons_pressed) {
                can_pause = true;
            }*/
        }
    }

}

}
