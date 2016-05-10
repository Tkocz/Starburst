namespace Fab5.Starburst.States {

    using Fab5.Engine;
    using Fab5.Engine.Components;
    using Fab5.Engine.Core;
    using Fab5.Engine.Subsystems;
    using Main_Menu.Entities;
    using Main_Menu.Subsystems;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Media;

    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class Pause_State : Game_State {
    private bool can_unpause = false;
    private float restore_vol;
        private Game_State last_state;
        private SpriteBatch sprite_batch;
        private SpriteFont font;
        private GraphicsDevice graphicsDevice;
        private float btnDelay = .5f;

        enum options {
            resume,
            quit
        };


        public Pause_State(Game_State last_state)
        {
            this.last_state = last_state;
        }
        public override void init() {
            /*Gamepad_Util.vibrate(0, 0.0f, 0.0f);
            Gamepad_Util.vibrate(1, 0.0f, 0.0f);
            Gamepad_Util.vibrate(2, 0.0f, 0.0f);
            Gamepad_Util.vibrate(3, 0.0f, 0.0f);*/
            graphicsDevice = Starburst.inst().GraphicsDevice;
            sprite_batch = new SpriteBatch(graphicsDevice);

            add_subsystems(
                new Menu_Input_Handler()
            );

            font = Starburst.inst().get_content<SpriteFont>("sector034");

            // h�mta inputs fr�n f�reg�ende state
            var inputs = last_state.get_entities_fast(typeof(Input));
            for(int i = 0; i < inputs.Count; i++) {
                var input = inputs[i].get_component<Input>();
                var gamepadPlayer = create_entity(Player.create_components(input));
            }

        restore_vol = MediaPlayer.Volume;
        MediaPlayer.Volume *= 0.4f;
    }

        public override void update(float t, float dt) {
            if(btnDelay >= 0)
                btnDelay -= dt;
            base.update(t, dt);
        }

        public override void draw(float t, float dt) {
            graphicsDevice.Clear(Color.Black);
            Viewport vp = sprite_batch.GraphicsDevice.Viewport;
            // gamla init-saker

            //draw-grejer
            var w = Starburst.inst().GraphicsMgr.PreferredBackBufferWidth;
            var h = Starburst.inst().GraphicsMgr.PreferredBackBufferHeight;

            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            GFX_Util.fill_rect(sprite_batch, new Rectangle(0, 0, w, h), Color.Black * 0.5f);

            var s = new[] { "one", "two", "three", "four" };

            var text_size = GFX_Util.measure_string("Paused");
            var tx = (w - text_size.X) * 0.5f;
            var ty = (h - text_size.Y) * 0.5f;

            var pl = last_state.get_entities_fast(typeof(Ship_Info));
            List<Entity> pList = new List<Entity>();
            for (int i = 0; i < pl.Count; i++) {
                if (pl[i].get_component<Score>() != null)
                    pList.Add(pl[i]);
            }

            int rowHeight = 50;
            int vertSpacing = 10;
            int totalScoreHeight = rowHeight * pList.Count+1 + vertSpacing*(pList.Count-1+1);
            int startY = (int)(vp.Height*.25f-totalScoreHeight*.5f);

            int horSpacing = 20;
            int nameWidth = 300;

            String killsHeader = "Kills";
            String deathsHeader = "Deaths";
            String scoreHeader = "Score";
            Vector2 killsSize = font.MeasureString(killsHeader);
            Vector2 deathsSize = font.MeasureString(deathsHeader);
            Vector2 scoreSize = font.MeasureString("999999");

            int totalScoreWidth = (int)(nameWidth + horSpacing + killsSize.X + horSpacing + deathsSize.X + horSpacing + scoreSize.X);
            int nameX = (int)(vp.Width * .5f - totalScoreWidth * .5f);
            int killsX = nameX + nameWidth + horSpacing;
            int deathsX = (int)(killsX + killsSize.X + horSpacing);
            int scoreX = (int)(deathsX + deathsSize.X + horSpacing);
            
            // header row
            // TODO: m�la en rektangel bakom

            //GFX_Util.draw_def_text(sprite_batch, "Player", nameX, startY);
            GFX_Util.draw_def_text(sprite_batch, killsHeader, killsX, startY);
            GFX_Util.draw_def_text(sprite_batch, deathsHeader, deathsX, startY);
            GFX_Util.draw_def_text(sprite_batch, scoreHeader, scoreX, startY);

            startY += rowHeight + vertSpacing;

            for (int p = 0; p < pList.Count; p++) {
                var player = pList[p];
                Score player_score = player.get_component<Score>();
                Ship_Info player_shipinfo = player.get_component<Ship_Info>();

                if (player_score == null || player_shipinfo.pindex >= 5) continue;

                int pad_size = 6;
                int rowY = startY + rowHeight * p + vertSpacing * p;

                GFX_Util.draw_def_text(sprite_batch, "Player " + s[player_shipinfo.pindex - 1], nameX, rowY);
                GFX_Util.draw_def_text(sprite_batch, player_score.num_kills.ToString(), killsX, rowY);
                GFX_Util.draw_def_text(sprite_batch, player_score.num_deaths.ToString(), deathsX, rowY);
                GFX_Util.draw_def_text(sprite_batch, player_score.display_score.ToString(), scoreX, rowY);

                //GFX_Util.fill_rect(sprite_batch, new Rectangle((int)xx - pad_size, rowY, (int)scoretext_size.X + (pad_size * 2), (int)scoretext_size.Y + (pad_size * 2)), Color.AliceBlue * 0.2f);


            }

            GFX_Util.draw_def_text(sprite_batch, "Paused", tx, ty);

            sprite_batch.End();





            //h�mta position fr�n f�rsta input-entiteten
            var players = get_entities_fast(typeof(Position));
            Position cursorPosition = players[0].get_component<Position>();

            //spritebatch
            sprite_batch.Begin();
            
            // resume
            String menuText = "Resume";
            Vector2 menuTextSize = font.MeasureString(menuText);
            int totalMenuHeight = (int)(menuTextSize.Y * 2 + vertSpacing * 1);
            startY = (int)(vp.Height * .75f - totalMenuHeight * .5f);

            sprite_batch.DrawString(font, menuText, new Vector2(vp.Width * .5f - menuTextSize.X * .5f, startY), (cursorPosition.y == (int)options.resume ? Color.Gold : Color.White));

            // quit
            menuText = "Quit";
            menuTextSize = font.MeasureString(menuText);
            sprite_batch.DrawString(font, menuText, new Vector2(vp.Width * .5f - menuTextSize.X * .5f, startY+menuTextSize.Y+vertSpacing), (cursorPosition.y == (int)options.quit ? Color.Gold : Color.White));

            sprite_batch.End();

        Thread.Sleep(10);
    }
        public override void on_message(string msg, dynamic data) {

            if (btnDelay <= 0) {
                if (msg.Equals("fullscreen")) {
                    btnDelay = .5f;
                    Starburst.inst().GraphicsMgr.ToggleFullScreen();
                }
                else if (msg.Equals("up")) {
                    var entities = Starburst.inst().get_entities_fast(typeof(Position));
                    Entity entity = entities[0];
                    var position = entity.get_component<Position>();
                    if (position.y > 0) {
                        position.y -= 1;
                        position.x = 0;
                        Starburst.inst().message("play_sound_asset", new { name = "menu_click" });
                    }
                }
                else if (msg.Equals("down")) {
                    var entities = Starburst.inst().get_entities_fast(typeof(Position));
                    Entity entity = entities[0];
                    var position = entity.get_component<Position>();
                    if (position.y < (int)options.quit) {
                        position.y += 1;
                        Starburst.inst().message("play_sound_asset", new { name = "menu_click" });
                    }
                }
                else if (msg.Equals("select")) {
                    var entities = Starburst.inst().get_entities_fast(typeof(Input));
                    Entity cursor = entities[0];
                    Position cursorPosition = cursor.get_component<Position>();
                    if (cursorPosition.y == (int)options.resume) {
                        resume();
                    }
                    else if(cursorPosition.y == (int)options.quit) {
                        Starburst.inst().Quit();
                    }
                }
                else if (msg.Equals("start")) {
                    resume();
                }
                else if (msg.Equals("escape")) {
                    resume();
                }
            }
        }
        private void resume() {
            MediaPlayer.Volume = restore_vol;
            Starburst.inst().leave_state();
        }

    }

}
