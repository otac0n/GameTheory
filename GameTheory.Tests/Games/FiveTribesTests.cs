// Copyright © 2016 John Gietzen.  All Rights Reserved.
// This source is subject to the MIT license.
// Please see license.md for more information.

namespace GameTheory.Tests.Games
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using GameTheory.Games.FiveTribes;
    using GameTheory.Games.FiveTribes.Djinns;
    using GameTheory.Games.FiveTribes.Moves;
    using GameTheory.Games.FiveTribes.Tiles;
    using GameTheory.Players;
    using NUnit.Framework;

    [TestFixture]
    public class FiveTribesTests
    {
        [Test]
        public void AssassinateLastMeeple_OnEmptySquare_YieldsPlaceCamelMove()
        {
            var template = new GameState(3);

            var playerA = template.Players[0];
            var playerB = template.Players[1];

            var src = new Point(2, 2);
            var dst = new Point(3, 2);
            var kil = new Point(3, 1);

            var sultanate = template.Sultanate
                .ConvertAll(sq => sq.With(meeples: EnumCollection<Meeple>.Empty))
                .SetItem(src, template.Sultanate[src].With(meeples: new EnumCollection<Meeple>(Meeple.Assassin)))
                .SetItem(dst, template.Sultanate[dst].With(meeples: new EnumCollection<Meeple>(Meeple.Assassin)))
                .SetItem(kil, template.Sultanate[kil].With(meeples: new EnumCollection<Meeple>(Meeple.Elder)));

            var state = template.With(
                bidOrderTrack: ImmutableQueue.Create(playerA),
                sultanate: sultanate,
                phase: Phase.PickUpMeeples);

            state = MakeMove(state, playerA, m => m.ToString() == "Pick up meeples at (2, 2)");
            state = MakeMove(state, playerA, m => m.ToString() == "Drop Assassin at (3, 2)");
            state = MakeMove(state, playerA, m => m.ToString() == "Pick up all Assassin at (3, 2)");
            state = MakeMove(state, playerA, m => m.ToString() == "Place a Camel at (3, 2)");
            state = MakeMove(state, playerA, m => m.ToString() == "Assissinate Elder at (3, 1)");
            state = MakeMove(state, playerA, m => m.ToString() == "Place a Camel at (3, 1)");

            Assert.That(state.Sultanate[kil].Owner, Is.EqualTo(playerA));
            Assert.That(state.Sultanate[dst].Owner, Is.EqualTo(playerA));
        }

        [Test(Description = "In 3- and 4-player games, each player takes 8 Camels...")]
        public void CamelLimit_InThreeOrFourPlayerGame_ReturnsEight([Values(3, 4)]int players)
        {
            var state = new GameState(players);

            Assert.That(state.CamelLimit, Is.EqualTo(8));
        }

        [Test(Description = "In 2-player games, one player takes 11 ... Camels...")]
        public void CamelLimit_InTwoPlayerGame_ReturnsEleven()
        {
            var state = new GameState(2);

            Assert.That(state.CamelLimit, Is.EqualTo(11));
        }

        [Test]
        public void ComplexAssassinationScenario_Always_GoesAccordingToPlan()
        {
            var template = new GameState(3);

            var playerA = template.Players[0];
            var playerB = template.Players[1];
            var playerC = template.Players[2];

            var src = new Point(2, 2);
            var dst = new Point(3, 2);

            var sultanate = template.Sultanate
                .ConvertAll(sq => sq.With(meeples: EnumCollection<Meeple>.Empty))
                .SetItem(src, template.Sultanate[src].With(meeples: new EnumCollection<Meeple>(Meeple.Assassin)))
                .SetItem(dst, template.Sultanate[dst].With(meeples: new EnumCollection<Meeple>(Meeple.Assassin, Meeple.Merchant)));

            var inventory = ImmutableDictionary.CreateBuilder<PlayerToken, Inventory>();
            inventory.Add(playerA, new Inventory().With(
                    djinns: ImmutableList.Create<Djinn>(AnunNak.Instance, Kandicha.Instance, Sibittis.Instance),
                    goldCoins: 0,
                    meeples: new EnumCollection<Meeple>(Meeple.Elder),
                    resources: new EnumCollection<Resource>(Resource.Slave, Resource.Slave, Resource.Slave)));
            inventory.Add(playerB, new Inventory().With(
                    djinns: ImmutableList.Create<Djinn>(Boaz.Instance),
                    meeples: new EnumCollection<Meeple>(Meeple.Elder),
                    goldCoins: 0));
            inventory.Add(playerC, new Inventory().With(
                    djinns: ImmutableList.Create<Djinn>(Nekir.Instance, Baal.Instance),
                    meeples: new EnumCollection<Meeple>(Meeple.Elder),
                    goldCoins: 0));

            var state = new GameState(
                additionalState: ImmutableDictionary<string, string>.Empty,
                assassinationTables: template.AssassinationTables,
                bag: template.Bag.Add(Meeple.Elder),
                bidOrderTrack: ImmutableQueue.Create(playerC, playerB, playerA),
                turnOrderTrack: template.TurnOrderTrack,
                sultanate: sultanate,
                djinnDiscards: template.DjinnDiscards,
                djinnPile: ImmutableList.Create<Djinn>(Haurvatat.Instance, Ibus.Instance, Shamhat.Instance),
                inHand: template.InHand,
                inventory: inventory.ToImmutable(),
                lastPoint: template.LastPoint,
                phase: Phase.PickUpMeeples,
                players: template.Players,
                previousPoint: template.PreviousPoint,
                resourceDiscards: template.ResourceDiscards,
                resourcePile: new EnumCollection<Resource>(Resource.Slave, Resource.Slave),
                scoreTables: template.ScoreTables,
                subsequentMovesFactory: null,
                visibleDjinns: ImmutableList<Djinn>.Empty,
                visibleResources: ImmutableList<Resource>.Empty);
            state = Transition(template, state);
            var playerCGoldCoins = state.Inventory[playerC].GoldCoins;

            state = MakeMove(state, playerA, m => m.ToString() == "Pick up meeples at (2, 2)");
            state = MakeMove(state, playerA, m => m.ToString() == "Drop Assassin at (3, 2)");
            state = state.MakeMove(state.GetAvailableMoves(playerA).Single(m => m.ToString() == "Pick up all Assassin at (3, 2)"));
            Assert.That(state.GetAvailableMoves(), Has.None.InstanceOf<AssassinatePlayerMove>().With.Property("Victim").EqualTo(playerB));
            Assert.That(state.GetAvailableMoves(), Has.Some.InstanceOf<AssassinatePlayerMove>().With.Property("Victim").EqualTo(playerC));

            state = MakeMove(state, playerA, m => m is PayMeeplesAndResourcesMove);
            state = MakeMove(state, playerA, m => m is DrawDjinnsMove);
            state = MakeMove(state, playerA, m => m.ToString() == "Take Ibus");
            state = MakeMove(state, playerA, m => state.MakeMove(m).GetAvailableMoves(playerA).Any(x => x is DoubleAssassinKillCountMove));
            state = MakeMove(state, playerA, m => m.ToString() == "Double the number of meeples your Assassins kill this turn");
            Assert.That(state.GetAvailableMoves(), Has.None.InstanceOf<AssassinatePlayerMove>().With.Property("Victim").EqualTo(playerB));
            Assert.That(state.GetAvailableMoves(), Has.Some.InstanceOf<AssassinatePlayerMove>().With.Property("Victim").EqualTo(playerC));

            state = MakeMove(state, playerA, m => state.MakeMove(m).GetAvailableMoves(playerA).Any(x => x is AddMeeplesMove));
            state = MakeMove(state, playerA, m => m.ToString() == "Draw 2 Meeples and place at (3, 1)");
            state = MakeMove(state, playerA, m => m.ToString() == "Assissinate Elder,Elder at (3, 1)");
            state = MakeMove(state, playerA, m => m.ToString() == "Place a Camel at (3, 1)");
            ShowInventory(state);
            Assert.That(state.GetAvailableMoves(), Has.None.InstanceOf<AddMeeplesMove>());
            Assert.That(state.GetAvailableMoves(), Has.None.InstanceOf<DoubleAssassinKillCountMove>());
            Assert.That(state.Inventory[playerC].GoldCoins, Is.EqualTo(playerCGoldCoins + 6));
        }

        [Test(Description = "Mix all 90 wooden Meeples in the bag, then grab and drop them in random sets of 3 on each Tile.")]
        public void ctor_Always_Adds90MeeplesToTheSultanate([Values(2, 3, 4)] int players)
        {
            var state = new GameState(players);

            var meeples = state.Sultanate.SelectMany(s => s.Meeples).GroupBy(m => m).ToDictionary(g => g.Key, g => g.Count());

            Assert.That(state.Sultanate.Select(s => s.Meeples), Has.All.Count.EqualTo(3));
            Assert.That(meeples[Meeple.Vizier], Is.EqualTo(16));
            Assert.That(meeples[Meeple.Elder], Is.EqualTo(20));
            Assert.That(meeples[Meeple.Builder], Is.EqualTo(18));
            Assert.That(meeples[Meeple.Merchant], Is.EqualTo(18));
            Assert.That(meeples[Meeple.Assassin], Is.EqualTo(18));
        }

        [Test(Description = "Each player also gets 50 Gold Coins...")]
        public void ctor_Always_AddsFiftyGoldCoinsToEachPlayersInventory([Values(2, 3, 4)] int players)
        {
            var state = new GameState(players);

            Assert.That(state.Inventory.Values, Has.All.Property("GoldCoins").EqualTo(50));
        }

        [Test(Description = "Mix all 30 Tiles and place them randomly face up, to form a rectangle of 5 by 6 Tiles: This is the Sultanate.")]
        public void ctor_Always_AddsThirtyTilesToTheSultanate([Values(2, 3, 4)] int players)
        {
            var state = new GameState(players);

            var tiles = state.Sultanate.Select(s => s.Tile).ToLookup(t => t.GetType());

            Assert.That(Sultanate.Width, Is.EqualTo(6));
            Assert.That(Sultanate.Height, Is.EqualTo(5));
            Assert.That(tiles[typeof(BigMarket)].Count(), Is.EqualTo(4));
            Assert.That(tiles[typeof(SmallMarket)].Count(), Is.EqualTo(8));
            Assert.That(tiles[typeof(Oasis)].Count(), Is.EqualTo(6));
            Assert.That(tiles[typeof(Village)].Count(), Is.EqualTo(5));
            Assert.That(tiles[typeof(SacredPlace)].Where(t => t.Value == 6).Count(), Is.EqualTo(4));
            Assert.That(tiles[typeof(SacredPlace)].Where(t => t.Value == 10).Count(), Is.EqualTo(1));
            Assert.That(tiles[typeof(SacredPlace)].Where(t => t.Value == 12).Count(), Is.EqualTo(1));
            Assert.That(tiles[typeof(SacredPlace)].Where(t => t.Value == 15).Count(), Is.EqualTo(1));
        }

        [Test(Description = "Shuffle all Resources cards to form a draw pile face down, then draw the top 9 cards and lay them face up...")]
        public void ctor_Always_DealsNineResourcesFromThePile([Values(2, 3, 4)] int players)
        {
            var state = new GameState(players);

            var resources = state.ResourcePile.Concat(state.VisibleResources).GroupBy(r => r).ToDictionary(g => g.Key, g => g.Count());

            Assert.That(state.VisibleResources, Has.Count.EqualTo(9));
            Assert.That(resources[Resource.Ivory], Is.EqualTo(2));
            Assert.That(resources[Resource.Jewels], Is.EqualTo(2));
            Assert.That(resources[Resource.Gold], Is.EqualTo(2));
            Assert.That(resources[Resource.Papyrus], Is.EqualTo(4));
            Assert.That(resources[Resource.Silk], Is.EqualTo(4));
            Assert.That(resources[Resource.Spice], Is.EqualTo(4));
            Assert.That(resources[Resource.Fish], Is.EqualTo(6));
            Assert.That(resources[Resource.Wheat], Is.EqualTo(6));
            Assert.That(resources[Resource.Pottery], Is.EqualTo(6));
            Assert.That(resources[Resource.Slave], Is.EqualTo(18));
        }

        [Test(Description = "Shuffle the Djinns' cards and set them face down in a draw pile, then draw the top 3 and lay them face up...")]
        public void ctor_Always_DealsThreeDjinnsFromThePile([Values(2, 3, 4)] int players)
        {
            var state = new GameState(players);

            Assert.That(state.VisibleDjinns, Has.Count.EqualTo(3));
            Assert.That(state.DjinnPile, Has.Count.EqualTo(19));
        }

        [Test(Description = "In 3- and 4-player games, each player takes ... 1 Turn marker... Take the players' Turn markers and randomly place them on the Bid Order track.")]
        public void ctor_InThreeOrFourPlayerGame_CreatesBidOrderTrackContainingOneEntryPerPlayer([Values(3, 4)]int players)
        {
            var state = new GameState(players);

            Assert.That(state.BidOrderTrack, Is.EquivalentTo(state.Players));
        }

        [Test(Description = "In 2-player games, one player takes ... 2 Turn markers... Take the players' Turn markers and randomly place them on the Bid Order track.")]
        public void ctor_InTwoPlayerGame_CreatesBidOrderTrackContainingTwoEntriesPerPlayer()
        {
            var state = new GameState(2);

            Assert.That(state.BidOrderTrack, Is.EquivalentTo(state.Players.Concat(state.Players)));
        }

        [Test(Description = "1 VP for each Gold Coin (GC) you won")]
        public void GetScore_WhenTheGameEnds_ReturnsOnePointForEachGoldCoin([Values(1, 5, 10, 25, 50)] int coins)
        {
            var state = new GameState(2);

            state = state.With(inventory: state.Inventory.Keys.ToImmutableDictionary(k => k, k => state.Inventory[k].With(goldCoins: coins)));

            Assert.That(state.Players.Select(p => state.GetScore(p)), Is.All.EqualTo(coins));
        }

        [Test(Description = "At the end of the game, the player with the most Victory Points is declared The Great Sultan and wins.")]
        public void GetWinners_AfterAGameHasBeenPlayed_ReturnsThePlayersWithTheHighestScore()
        {
            var endState = (GameState)GameUtils.PlayGame(
                new GameState(2),
                p => new RandomPlayer<Move>(p),
                (state, move) => Console.WriteLine("{0}: {1}", FiveTribesTests.p((GameState)state, move.Player), move)).Result;

            var highestScore = endState.Players.Max(p => endState.GetScore(p));
            var winners = endState.GetWinners();

            Assert.That(winners, Is.EqualTo(endState.Players.Where(p => endState.GetScore(p) == highestScore)));
        }

        private static CancellationToken GetCancellationToken()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            return cts.Token;
        }

        private static GameState MakeMove(GameState state, PlayerToken player, Expression<Func<Move, bool>> filter)
        {
            var moves = state.GetAvailableMoves(player).Where(filter.Compile()).ToList();
            if (moves.Count != 1)
            {
                ShowMoves(state);
                Console.WriteLine("Filter: {0}", filter);
            }

            return state.MakeMove(moves.Single());
        }

        private static string p(GameState state, PlayerToken player)
        {
            return ((char)('A' + state.Players.IndexOf(player))).ToString();
        }

        private static void ShowInventory(GameState state)
        {
            Console.WriteLine("Inventories:");
            foreach (var player in state.Players)
            {
                var inventory = state.Inventory[player];
                var stuff = new List<object>();
                stuff.AddRange(inventory.Meeples.Cast<object>());
                stuff.AddRange(inventory.Resources.Cast<object>());
                stuff.AddRange(inventory.Djinns);
                Console.WriteLine("{0}: {1} {2} ", p(state, player), inventory.GoldCoins, string.Join(",", stuff));
            }
        }

        private static void ShowMoves(GameState state)
        {
            Console.WriteLine("Available Moves:");
            foreach (var move in state.GetAvailableMoves())
            {
                Console.WriteLine("{0}: {1} ", p(state, move.Player), move);
            }
        }

        private static void ShowScores(GameState state)
        {
            Console.WriteLine("Scores:");
            foreach (var player in state.Players)
            {
                Console.WriteLine("{0}: {1} ", p(state, player), state.GetScore(player));
            }
        }

        private static void ShowWinners(GameState state)
        {
            var winners = state.GetWinners();
            Console.WriteLine("Winners: {0}", string.Join(", ", winners.Select(winner => p(state, winner))));
        }

        private static GameState Transition(GameState from, GameState to)
        {
            var move = new ReturnGameStateMove(from, to);
            return from.MakeMove(move);
        }

        public class ReturnGameStateMove : Move
        {
            private readonly GameState nextState;

            public ReturnGameStateMove(GameState state, GameState nextState)
                : base(state, state.ActivePlayer)
            {
                this.nextState = nextState;
            }

            public override string ToString()
            {
                return "OK";
            }

            internal override GameState Apply(GameState state)
            {
                return this.nextState;
            }
        }
    }
}
