using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukesScripts.Blueprints
{
    public static class EventHooks
    {
        // AI and Weapons
        /// <summary>
        /// No parameters
        /// </summary>
        public static string Init = "Init";
        /// <summary>
        /// No parameters
        /// </summary>
        public static string Tick = "Tick";

        // Weapons
        /// <summary>
        /// 6 Parameters: 0 = Seconds between shots  - float
        ///               1 = Projectile spawn point - Vector3
        ///               2 = y rotation             - float
        ///               3 = angle                  - float
        ///               4 = uses infinite ammo     - bool
        ///               5 = is in player inventory - bool
        /// </summary>
        public static string Fire = "Fire";

        // AI
        /// <summary>
        /// No parameters
        /// </summary>
        public static string Attack = "Attack";
        /// <summary>
        /// No parameters
        /// </summary>
        public static string OnHit = "OnHit";
        /// <summary>
        /// No parameters
        /// </summary>
        public static string OnDeath = "OnDeath";

        // External - Unity functions
        /// <summary>
        /// No parameters
        /// </summary>
        public static string DrawGizmos = "DrawGizmos";

        // Inventory
        /// <summary>
        /// No parameters
        /// </summary>
        public static string OnWeaponDropped = "OnWeaponDropped";
        /// <summary>
        /// No parameters
        /// </summary>
        public static string OnWeaponGrabbed = "OnWeaponGrabbed";
        /// <summary>
        /// 2 Parameters: 0 = remaining ammo             - int
        ///               1 = weapon on ground is empty  - bool
        /// </summary>
        public static string OnAmmoReplenished = "OnAmmoReplenished";
        /// <summary>
        /// No parameters
        /// </summary>
        public static string OnAmmoFull = "OnAmmoFull";
        /// <summary>
        /// No parameters
        /// </summary>
        public static string InventoryFull = "InventoryFull";
    }
}
