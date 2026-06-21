namespace NightLum.Elevate.Composition {
    /// <summary>Identifies how a layer is combined with the accumulated value.</summary>
    public enum BlendMode {
        /// <summary>Adds the weighted layer value.</summary>
        Add,
        /// <summary>Interpolates from the current value to the layer value.</summary>
        Blend,
        /// <summary>Moves toward the greater value.</summary>
        Max,
        /// <summary>Moves toward the lesser value.</summary>
        Min
    }
}
