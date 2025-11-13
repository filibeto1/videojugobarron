using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player is spawned after dying.
    /// </summary>
    public class PlayerSpawn : Simulation.Event<PlayerSpawn>
    {
        public override void Execute()
        {
            var player = Simulation.GetModel<PlatformerModel>().player;

            // Habilitar el collider del jugador
            player.collider2d.enabled = true;

            // Desactivar el control temporalmente
            player.controlEnabled = false;

            // Incrementar la salud del jugador
            player.health.Increment();

            // Teletransportar al jugador a la posición de inicio
            player.Teleport(Simulation.GetModel<PlatformerModel>().spawnPoint.transform.position);

            // Establecer el estado de salto del jugador
            player.GetComponent<PlayerController>().jumpState = PlayerController.JumpState.Grounded;

            // Detener la animación de muerte
            player.animator.SetBool("dead", false);

            // Configurar la cámara virtual para seguir al jugador
            var virtualCamera = Simulation.GetModel<PlatformerModel>().virtualCamera;
            virtualCamera.m_Follow = player.transform;
            virtualCamera.m_LookAt = player.transform;

            // Programar el evento para habilitar el control del jugador después de un retraso
            Simulation.Schedule<EnablePlayerInput>(2f);
        }
    }
}
