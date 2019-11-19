﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GameTheory.Games.Chess.Uci {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("GameTheory.Games.Chess.Uci.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hash Table Size (MB).
        /// </summary>
        public static string Hash {
            get {
                return ResourceManager.GetString("Hash", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The size (in MB) of the engine&apos;s hash tables..
        /// </summary>
        public static string HashDescription {
            get {
                return ResourceManager.GetString("HashDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Multiple Principle Variations.
        /// </summary>
        public static string MultiPV {
            get {
                return ResourceManager.GetString("MultiPV", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The number of best lines..
        /// </summary>
        public static string MultiPVDescription {
            get {
                return ResourceManager.GetString("MultiPVDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Nalimov Cache Size (MB).
        /// </summary>
        public static string NalimovCache {
            get {
                return ResourceManager.GetString("NalimovCache", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The size (in MB) of the engine&apos;s endgame tablebase cache..
        /// </summary>
        public static string NalimovCacheDescription {
            get {
                return ResourceManager.GetString("NalimovCacheDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Nalimov Path.
        /// </summary>
        public static string NalimovPath {
            get {
                return ResourceManager.GetString("NalimovPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The path to a directory containing endgame tablebases in the Nalimov format. Multiple directories must be separated with a semicolon (&quot;;&quot;)..
        /// </summary>
        public static string NalimovPathDescription {
            get {
                return ResourceManager.GetString("NalimovPathDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use Book.
        /// </summary>
        public static string OwnBook {
            get {
                return ResourceManager.GetString("OwnBook", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The engine may access its opening book..
        /// </summary>
        public static string OwnBookDescription {
            get {
                return ResourceManager.GetString("OwnBookDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ponder.
        /// </summary>
        public static string Ponder {
            get {
                return ResourceManager.GetString("Ponder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Whether the engine is allowed to ponder..
        /// </summary>
        public static string PonderDescription {
            get {
                return ResourceManager.GetString("PonderDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Syzygy 50-move Rule.
        /// </summary>
        public static string Syzygy50MoveRule {
            get {
                return ResourceManager.GetString("Syzygy50MoveRule", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Whether to include the 50-move rule in the evaluation of endgame tablebase positions..
        /// </summary>
        public static string Syzygy50MoveRuleDescription {
            get {
                return ResourceManager.GetString("Syzygy50MoveRuleDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Syzygy Path.
        /// </summary>
        public static string SyzygyPath {
            get {
                return ResourceManager.GetString("SyzygyPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The path to a directory containing endgame tablebases in the Syzygy format..
        /// </summary>
        public static string SyzygyPathDescription {
            get {
                return ResourceManager.GetString("SyzygyPathDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Syzygy Probe Depth.
        /// </summary>
        public static string SyzygyProbeDepth {
            get {
                return ResourceManager.GetString("SyzygyProbeDepth", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The minimum search depth that must be reached before the endgame tablebase will be probed. The starting position of each move will always be probed..
        /// </summary>
        public static string SyzygyProbeDepthDescription {
            get {
                return ResourceManager.GetString("SyzygyProbeDepthDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Syzygy Probe Limit.
        /// </summary>
        public static string SyzygyProbeLimit {
            get {
                return ResourceManager.GetString("SyzygyProbeLimit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The maximum number of pieces remaining before probing the endgame tablebase..
        /// </summary>
        public static string SyzygyProbeLimitDescription {
            get {
                return ResourceManager.GetString("SyzygyProbeLimitDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to About.
        /// </summary>
        public static string UCI_About {
            get {
                return ResourceManager.GetString("UCI_About", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Analyze Mode.
        /// </summary>
        public static string UCI_AnalyseMode {
            get {
                return ResourceManager.GetString("UCI_AnalyseMode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Whether the engine is in &quot;analyze mode&quot;..
        /// </summary>
        public static string UCI_AnalyseModeDescription {
            get {
                return ResourceManager.GetString("UCI_AnalyseModeDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Chess960 Castling.
        /// </summary>
        public static string UCI_Chess960 {
            get {
                return ResourceManager.GetString("UCI_Chess960", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Whether castling moves will be represented as the capture of a rook, to support Chess960..
        /// </summary>
        public static string UCI_Chess960Description {
            get {
                return ResourceManager.GetString("UCI_Chess960Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ELO.
        /// </summary>
        public static string UCI_Elo {
            get {
                return ResourceManager.GetString("UCI_Elo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The strength to which the engine should limit itself..
        /// </summary>
        public static string UCI_EloDescription {
            get {
                return ResourceManager.GetString("UCI_EloDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Limit Strength.
        /// </summary>
        public static string UCI_LimitStrength {
            get {
                return ResourceManager.GetString("UCI_LimitStrength", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Whether the engine should limit its playing strength..
        /// </summary>
        public static string UCI_LimitStrengthDescription {
            get {
                return ResourceManager.GetString("UCI_LimitStrengthDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Opponent.
        /// </summary>
        public static string UCI_Opponent {
            get {
                return ResourceManager.GetString("UCI_Opponent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The title, ELO, and name of the opponent..
        /// </summary>
        public static string UCI_OpponentDescription {
            get {
                return ResourceManager.GetString("UCI_OpponentDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show Current Line.
        /// </summary>
        public static string UCI_ShowCurrLine {
            get {
                return ResourceManager.GetString("UCI_ShowCurrLine", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Whether the engine should show the line it is calculating..
        /// </summary>
        public static string UCI_ShowCurrLineDescription {
            get {
                return ResourceManager.GetString("UCI_ShowCurrLineDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show Refutations.
        /// </summary>
        public static string UCI_ShowRefutations {
            get {
                return ResourceManager.GetString("UCI_ShowRefutations", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Whether the engine should a move and its refutation in a line..
        /// </summary>
        public static string UCI_ShowRefutationsDescription {
            get {
                return ResourceManager.GetString("UCI_ShowRefutationsDescription", resourceCulture);
            }
        }
    }
}
