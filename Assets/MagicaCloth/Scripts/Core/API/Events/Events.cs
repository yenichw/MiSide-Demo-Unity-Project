// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp

namespace MagicaCloth
{
    /// <summary>
    /// ?????????????
    /// Avatar parts attach event.
    /// </summary>
    [System.Serializable]
    public class AvatarPartsAttachEvent : UnityEngine.Events.UnityEvent<MagicaAvatar, MagicaAvatarParts>
    {
    }

    /// <summary>
    /// ?????????????
    /// Avatar parts detach event.
    /// </summary>
    [System.Serializable]
    public class AvatarPartsDetachEvent : UnityEngine.Events.UnityEvent<MagicaAvatar>
    {
    }

    /// <summary>
    /// ????????????
    /// </summary>
    [System.Serializable]
    public class PhysicsManagerPreUpdateEvent : UnityEngine.Events.UnityEvent
    {
    }

    /// <summary>
    /// ????????????
    /// </summary>
    [System.Serializable]
    public class PhysicsManagerPostUpdateEvent : UnityEngine.Events.UnityEvent
    {
    }
}
