namespace Starburst {

using Engine;
using Engine.Core;

using Starburst.Entities;

using Microsoft.Xna.Framework.Graphics;

public class Starburst_Game_Impl : Game_Impl {
    public override void init() {
        Game_Engine.inst().add_subsystems(
            new Engine.Subsystems.Position_Integrator(),
            new Engine.Subsystems.Sprite_Renderer(new SpriteBatch(Game_Engine.inst().GraphicsDevice))
        );

        Game_Engine.inst().entities.Add(new Ball());
    }
}

}
