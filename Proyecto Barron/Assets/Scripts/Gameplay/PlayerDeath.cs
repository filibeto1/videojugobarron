using System.Collections;
using System.Collections.Generic;
using Platformer.Core;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player has died.
    /// </summary>
    /// <typeparam name="PlayerDeath"></typeparam>
    public class PlayerDeath : Simulation.Event<PlayerDeath>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            var player = model.player;
            if (player.health.IsAlive)
            {
                // Marcar al jugador como muerto
                player.health.Die();

                // Detener el seguimiento de la cámara del jugador
                model.virtualCamera.m_Follow = null;
                model.virtualCamera.m_LookAt = null;

                // Deshabilitar el control del jugador
                player.controlEnabled = false;

                // Realizar la animación de herido y muerte
                player.animator.SetTrigger("hurt");
                player.animator.SetBool("dead", true);

                // Programar el evento de reaparición después de un tiempo
                Simulation.Schedule<PlayerSpawn>(2);
            }
        }
    }
}
