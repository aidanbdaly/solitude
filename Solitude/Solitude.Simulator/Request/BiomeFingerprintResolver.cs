using Solitude.Simulator.Core.Model;

public class BiomeFingerprintResolver : Dictionary<Biome, Func<Biome, BiomeFingerprint>>;