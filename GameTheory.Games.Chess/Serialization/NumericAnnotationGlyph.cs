// Copyright © John & Katie Gietzen. All Rights Reserved. This source is subject to the MIT license. Please see license.md for more information.

namespace GameTheory.Games.Chess.Serialization
{
    /// <summary>
    /// A predefined set of annotations for chess games.
    /// </summary>
    /// <remarks>
    /// Descriptions from <see href="https://en.wikipedia.org/wiki/Portable_Game_Notation#Numeric_Annotation_Glyphs"/>.
    /// </remarks>
    public enum NumericAnnotationGlyph
    {
        /// <summary>
        /// The null annotation.
        /// </summary>
        None = 0,

        /// <summary>A good move (traditional "!").</summary>
        Good = 1,

        /// <summary>A poor move or mistake (traditional "?").</summary>
        Inaccurate = 2,

        /// <summary>A very good or brilliant move (traditional "!!").</summary>
        Brilliant = 3,

        /// <summary>A very poor move or blunder (traditional "??").</summary>
        Blunder = 4,

        /// <summary>A speculative or interesting move (traditional "!?").</summary>
        Interesting = 5,

        /// <summary>A questionable or dubious move (traditional "?!").</summary>
        Dubious = 6,

        /// <summary>A forced move (all others lose quickly) or the only move.</summary>
        Forced = 7,

        /// <summary>A singular move (no reasonable alternatives).</summary>
        Singular = 8,

        /// <summary>The worst move.</summary>
        Worst = 9,

        /// <summary>A drawish position or even.</summary>
        Even = 10,

        /// <summary>Equal chances, quiet position.</summary>
        Quiet = 11,

        /// <summary>Equal chances, active position.</summary>
        Active = 12,

        /// <summary>An unclear position.</summary>
        Unclear = 13,

        /// <summary>White has a slight advantage.</summary>
        WhiteSlightAdvantage = 14,

        /// <summary>Black has a slight advantage.</summary>
        BlackSlightAdvantage = 15,

        /// <summary>White has a moderate advantage.</summary>
        WhiteModerateAdvantage = 16,

        /// <summary>Black has a moderate advantage.</summary>
        BlackModerateAdvantage = 17,

        /// <summary>White has a decisive advantage.</summary>
        WhiteDecisiveAdvantage = 18,

        /// <summary>Black has a decisive advantage.</summary>
        BlackDecisiveAdvantage = 19,

        /// <summary>White has a crushing advantage. Black should resign.</summary>
        WhiteCrushingAdvantage = 20,

        /// <summary>Black has a crushing advantage. White should resign.</summary>
        BlackCrushingAdvantage = 21,

        /// <summary>White is in zugzwang.</summary>
        WhiteZugzwang = 22,

        /// <summary>Black is in zugzwang.</summary>
        BlackZugzwang = 23,

        /// <summary>White has a slight space advantage.</summary>
        WhiteSlightSpaceAdvantage = 24,

        /// <summary>Black has a slight space advantage.</summary>
        BlackSlightSpaceAdvantage = 25,

        /// <summary>White has a moderate space advantage.</summary>
        WhiteModerateSpaceAdvantage = 26,

        /// <summary>Black has a moderate space advantage.</summary>
        BlackModerateSpaceAdvantage = 27,

        /// <summary>White has a decisive space advantage.</summary>
        WhiteDecisiveSpaceAdvantage = 28,

        /// <summary>Black has a decisive space advantage.</summary>
        BlackDecisiveSpaceAdvantage = 29,

        /// <summary>White has a slight development advantage.</summary>
        WhiteSlightDevelopmentAdvantage = 30,

        /// <summary>Black has a slight development advantage.</summary>
        BlackSlightDevelopmentAdvantage = 31,

        /// <summary>White has a moderate development advantage.</summary>
        WhiteModerateDevelopmentAdvantage = 32,

        /// <summary>Black has a moderate development advantage.</summary>
        BlackModerateDevelopmentAdvantage = 33,

        /// <summary>White has a decisive development advantage.</summary>
        WhiteDecisiveDevelopmentAdvantage = 34,

        /// <summary>Black has a decisive development advantage.</summary>
        BlackDecisiveDevelopmentAdvantage = 35,

        /// <summary>White has the initiative.</summary>
        WhiteInitiative = 36,

        /// <summary>Black has the initiative.</summary>
        BlackInitiative = 37,

        /// <summary>White has a lasting initiative.</summary>
        WhiteLastingInitiative = 38,

        /// <summary>Black has a lasting initiative.</summary>
        BlackLastingInitiative = 39,

        /// <summary>White has the attack.</summary>
        WhiteAttack = 40,

        /// <summary>Black has the attack.</summary>
        BlackAttack = 41,

        /// <summary>White has insufficient compensation for material deficit.</summary>
        WhiteInsufficientCompensationForMaterialDeficit = 42,

        /// <summary>Black has insufficient compensation for material deficit.</summary>
        BlackInsufficientCompensationForMaterialDeficit = 43,

        /// <summary>White has sufficient compensation for material deficit.</summary>
        WhiteSufficientCompensationForMaterialDeficit = 44,

        /// <summary>Black has sufficient compensation for material deficit.</summary>
        BlackSufficientCompensationForMaterialDeficit = 45,

        /// <summary>White has more than adequate compensation for material deficit.</summary>
        WhiteMoreThanAdequateCompensationForMaterialDeficit = 46,

        /// <summary>Black has more than adequate compensation for material deficit.</summary>
        BlackMoreThanAdequateCompensationForMaterialDeficit = 47,

        /// <summary>White has a slight center control advantage.</summary>
        WhiteSlightCenterControlAdvantage = 48,

        /// <summary>Black has a slight center control advantage.</summary>
        BlackSlightCenterControlAdvantage = 49,

        /// <summary>White has a moderate center control advantage.</summary>
        WhiteModerateCenterControlAdvantage = 50,

        /// <summary>Black has a moderate center control advantage.</summary>
        BlackModerateCenterControlAdvantage = 51,

        /// <summary>White has a decisive center control advantage.</summary>
        WhiteDecisiveCenterControlAdvantage = 52,

        /// <summary>Black has a decisive center control advantage.</summary>
        BlackDecisiveCenterControlAdvantage = 53,

        /// <summary>White has a slight kingside control advantage.</summary>
        WhiteSlightKingsideControlAdvantage = 54,

        /// <summary>Black has a slight kingside control advantage.</summary>
        BlackSlightKingsideControlAdvantage = 55,

        /// <summary>White has a moderate kingside control advantage.</summary>
        WhiteModerateKingsideControlAdvantage = 56,

        /// <summary>Black has a moderate kingside control advantage.</summary>
        BlackModerateKingsideControlAdvantage = 57,

        /// <summary>White has a decisive kingside control advantage.</summary>
        WhiteDecisiveKingsideControlAdvantage = 58,

        /// <summary>Black has a decisive kingside control advantage.</summary>
        BlackDecisiveKingsideControlAdvantage = 59,

        /// <summary>White has a slight queenside control advantage.</summary>
        WhiteSlightQueensideControlAdvantage = 60,

        /// <summary>Black has a slight queenside control advantage.</summary>
        BlackSlightQueensideControlAdvantage = 61,

        /// <summary>White has a moderate queenside control advantage.</summary>
        WhiteModerateQueensideControlAdvantage = 62,

        /// <summary>Black has a moderate queenside control advantage.</summary>
        BlackModerateQueensideControlAdvantage = 63,

        /// <summary>White has a decisive queenside control advantage.</summary>
        WhiteDecisiveQueensideControlAdvantage = 64,

        /// <summary>Black has a decisive queenside control advantage.</summary>
        BlackDecisiveQueensideControlAdvantage = 65,

        /// <summary>White has a vulnerable first rank.</summary>
        WhiteVulnerableFirstRank = 66,

        /// <summary>Black has a vulnerable first rank.</summary>
        BlackVulnerableFirstRank = 67,

        /// <summary>White has a well protected first rank.</summary>
        WhiteWellProtectedFirstRank = 68,

        /// <summary>Black has a well protected first rank.</summary>
        BlackWellProtectedFirstRank = 69,

        /// <summary>White has a poorly protected king.</summary>
        WhitePoorlyProtectedKing = 70,

        /// <summary>Black has a poorly protected king.</summary>
        BlackPoorlyProtectedKing = 71,

        /// <summary>White has a well protected king.</summary>
        WhiteWellProtectedKing = 72,

        /// <summary>Black has a well protected king.</summary>
        BlackWellProtectedKing = 73,

        /// <summary>White has a poorly placed king.</summary>
        WhitePoorlyPlacedKing = 74,

        /// <summary>Black has a poorly placed king.</summary>
        BlackPoorlyPlacedKing = 75,

        /// <summary>White has a well placed king.</summary>
        WhiteWellPlacedKing = 76,

        /// <summary>Black has a well placed king.</summary>
        BlackWellPlacedKing = 77,

        /// <summary>White has a very weak pawn structure.</summary>
        WhiteVeryWeakPawnStructure = 78,

        /// <summary>Black has a very weak pawn structure.</summary>
        BlackVeryWeakPawnStructure = 79,

        /// <summary>White has a moderately weak pawn structure.</summary>
        WhiteModeratelyWeakPawnStructure = 80,

        /// <summary>Black has a moderately weak pawn structure.</summary>
        BlackModeratelyWeakPawnStructure = 81,

        /// <summary>White has a moderately strong pawn structure.</summary>
        WhiteModeratelyStrongPawnStructure = 82,

        /// <summary>Black has a moderately strong pawn structure.</summary>
        BlackModeratelyStrongPawnStructure = 83,

        /// <summary>White has a very strong pawn structure.</summary>
        WhiteVeryStrongPawnStructure = 84,

        /// <summary>Black has a very strong pawn structure.</summary>
        BlackVeryStrongPawnStructure = 85,

        /// <summary>White has poor knight placement.</summary>
        WhitePoorKnightPlacement = 86,

        /// <summary>Black has poor knight placement.</summary>
        BlackPoorKnightPlacement = 87,

        /// <summary>White has good knight placement.</summary>
        WhiteGoodKnightPlacement = 88,

        /// <summary>Black has good knight placement.</summary>
        BlackGoodKnightPlacement = 89,

        /// <summary>White has poor bishop placement.</summary>
        WhitePoorBishopPlacement = 90,

        /// <summary>Black has poor bishop placement.</summary>
        BlackPoorBishopPlacement = 91,

        /// <summary>White has good bishop placement.</summary>
        WhiteGoodBishopPlacement = 92,

        /// <summary>Black has good bishop placement.</summary>
        BlackGoodBishopPlacement = 93,

        /// <summary>White has poor rook placement.</summary>
        WhitePoorRookPlacement = 94,

        /// <summary>Black has poor rook placement.</summary>
        BlackPoorRookPlacement = 95,

        /// <summary>White has good rook placement.</summary>
        WhiteGoodRookPlacement = 96,

        /// <summary>Black has good rook placement.</summary>
        BlackGoodRookPlacement = 97,

        /// <summary>White has poor queen placement.</summary>
        WhitePoorQueenPlacement = 98,

        /// <summary>Black has poor queen placement.</summary>
        BlackPoorQueenPlacement = 99,

        /// <summary>White has good queen placement.</summary>
        WhiteGoodQueenPlacement = 100,

        /// <summary>Black has good queen placement.</summary>
        BlackGoodQueenPlacement = 101,

        /// <summary>White has poor piece coordination.</summary>
        WhitePoorPieceCoordination = 102,

        /// <summary>Black has poor piece coordination.</summary>
        BlackPoorPieceCoordination = 103,

        /// <summary>White has good piece coordination.</summary>
        WhiteGoodPieceCoordination = 104,

        /// <summary>Black has good piece coordination.</summary>
        BlackGoodPieceCoordination = 105,

        /// <summary>White has played the opening very poorly.</summary>
        WhitePlayedOpeningVeryPoorly = 106,

        /// <summary>Black has played the opening very poorly.</summary>
        BlackPlayedOpeningVeryPoorly = 107,

        /// <summary>White has played the opening poorly.</summary>
        WhitePlayedOpeningPoorly = 108,

        /// <summary>Black has played the opening poorly.</summary>
        BlackPlayedOpeningPoorly = 109,

        /// <summary>White has played the opening well.</summary>
        WhitePlayedOpeningWell = 110,

        /// <summary>Black has played the opening well.</summary>
        BlackPlayedOpeningWell = 111,

        /// <summary>White has played the opening very well.</summary>
        WhitePlayedOpeningVeryWell = 112,

        /// <summary>Black has played the opening very well.</summary>
        BlackPlayedOpeningVeryWell = 113,

        /// <summary>White has played the middlegame very poorly.</summary>
        WhitePlayedMiddlegameVeryPoorly = 114,

        /// <summary>Black has played the middlegame very poorly.</summary>
        BlackPlayedMiddlegameVeryPoorly = 115,

        /// <summary>White has played the middlegame poorly.</summary>
        WhitePlayedMiddlegamePoorly = 116,

        /// <summary>Black has played the middlegame poorly.</summary>
        BlackPlayedMiddlegamePoorly = 117,

        /// <summary>White has played the middlegame well.</summary>
        WhitePlayedMiddlegameWell = 118,

        /// <summary>Black has played the middlegame well.</summary>
        BlackPlayedMiddlegameWell = 119,

        /// <summary>White has played the middlegame very well.</summary>
        WhitePlayedMiddlegameVeryWell = 120,

        /// <summary>Black has played the middlegame very well.</summary>
        BlackPlayedMiddlegameVeryWell = 121,

        /// <summary>White has played the ending very poorly.</summary>
        WhitePlayedEndingVeryPoorly = 122,

        /// <summary>Black has played the ending very poorly.</summary>
        BlackPlayedEndingVeryPoorly = 123,

        /// <summary>White has played the ending poorly.</summary>
        WhitePlayedEndingPoorly = 124,

        /// <summary>Black has played the ending poorly.</summary>
        BlackPlayedEndingPoorly = 125,

        /// <summary>White has played the ending well.</summary>
        WhitePlayedEndingWell = 126,

        /// <summary>Black has played the ending well.</summary>
        BlackPlayedEndingWell = 127,

        /// <summary>White has played the ending very well.</summary>
        WhitePlayedEndingVeryWell = 128,

        /// <summary>Black has played the ending very well.</summary>
        BlackPlayedEndingVeryWell = 129,

        /// <summary>White has slight counterplay.</summary>
        WhiteSlightCounterplay = 130,

        /// <summary>Black has slight counterplay.</summary>
        BlackSlightCounterplay = 131,

        /// <summary>White has moderate counterplay.</summary>
        WhiteModerateCounterplay = 132,

        /// <summary>Black has moderate counterplay.</summary>
        BlackModerateCounterplay = 133,

        /// <summary>White has decisive counterplay.</summary>
        WhiteDecisiveCounterplay = 134,

        /// <summary>Black has decisive counterplay.</summary>
        BlackDecisiveCounterplay = 135,

        /// <summary>White has moderate time control pressure</summary>
        WhiteModerateTimePressure = 136,

        /// <summary>Black has moderate time control pressure</summary>
        BlackModerateTimePressure = 137,

        /// <summary>White has severe time control pressure.</summary>
        WhiteSevereTimePressure = 138,

        /// <summary>Black has severe time control pressure.</summary>
        BlackSevereTimePressure = 139,
    }
}
