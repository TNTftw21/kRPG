﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace kRPG2
{
    public class BaseGui
    {
        public BaseGui()
        {
            GuiElements.Add(this);
        }

        public List<InterfaceButton> Buttons { get; set; } = new List<InterfaceButton>();

        public bool GuiActive { get; set; }
        public static List<BaseGui> GuiElements { get; set; } = new List<BaseGui>();

        public virtual bool RemoveOnClose => false;

        public InterfaceButton AddButton(Func<Rectangle> position, Action<Player> pressAction)
        {
            var button = new InterfaceButton(position, pressAction);
            Buttons.Add(button);
            return button;
        }

        public InterfaceButton AddButton(Func<Rectangle> position, Action<Player> pressAction, Action<Player, SpriteBatch> hoverAction)
        {
            var button = new InterfaceButton(position, pressAction, hoverAction);
            Buttons.Add(button);
            return button;
        }

        public void CloseGui()
        {
            OnClose();
            GuiActive = false;
            if (RemoveOnClose) GuiElements.Remove(this);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Player player)
        {
            PostDraw(spriteBatch, player);

            foreach (var button in Buttons)
                button.Update(spriteBatch, player);
        }

        public virtual void OnClose()
        {
        }

        public virtual void PostDraw(SpriteBatch spriteBatch, Player player)
        {
        }

        public virtual bool PreDraw()
        {
            return GuiActive;
        }

        public void RemoveButton(InterfaceButton button)
        {
            Buttons.Remove(button);
        }

        //public virtual void PostDraw(SpriteBatch spriteBatch, Player player) {}
    }
}