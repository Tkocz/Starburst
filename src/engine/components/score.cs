﻿namespace Fab5.Engine.Components
{

    /*------------------------------------------------
     * USINGS
     *----------------------------------------------*/

    using Fab5.Engine.Core;


    /*------------------------------------------------
     * CLASSES
     *----------------------------------------------*/

    public class Score : Component
    {
        public int score;
        public int display_score;
        public Score()
        {
            score = 0;
        }
    }

}