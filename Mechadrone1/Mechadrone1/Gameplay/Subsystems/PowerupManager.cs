#region File Description
//-----------------------------------------------------------------------------
// PowerupManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Mechadrone1.Gameplay;
#endregion

namespace Mechadrone1.Gameplay
{
    class PowerupManager
    {
        Game1Manager game;        // game manager

        // linked list of active powerups
        LinkedList<Powerup> powerups;

        // linked list of nodes to delete from the powerups list
        List<LinkedListNode<Powerup>> deletePowerups;

        /// <summary>
        /// Create a new powerup manager
        /// </summary>
        public PowerupManager(Game1Manager game)
        {
            this.game = game;

            powerups = new LinkedList<Powerup>();
            deletePowerups = new List<LinkedListNode<Powerup>>();
        }

        /// <summary>
        /// Add a new powerup
        /// </summary>
        public void Add(Powerup p)
        {
            powerups.AddLast(p);
        }

        /// <summary>
        /// Empty powerups list
        /// </summary>
        public void Clear()
        {
            powerups.Clear();
        }

        /// <summary>
        /// Update all powerups
        /// </summary>
        public void Update(float elapsedTime)
        {
            // empty deleted powerups list
            deletePowerups.Clear();

            // for each powerup
            LinkedListNode<Powerup> Node = powerups.First;
            while(Node != null)
            {
                // update powerup
                bool running = Node.Value.Update(game, elapsedTime);

                // if finished running add to delete list
                if (running == false)
                    deletePowerups.Add(Node);
                
                // move to next node
                Node = Node.Next;
            }

            // delete all nodes in delete list
            foreach (LinkedListNode<Powerup> s in deletePowerups)
                powerups.Remove(s);
        }

    }


    public enum PowerupType
    {
        Energy = 0,                // 50% energy
        Missile                    // 3 missiles
    }

}

