using AdventOfCodeScaffolding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2020.Challenges.Day12
{
	[Challenge(12, "Rain Risk")]
	class Day12Challenge : ChallengeBase
	{
/*

Part 1:

Start at (0,0), Heading East
NSEW -> move in dir by value
LR -> rotate heading by degrees
F -> move forward along heading by value

Part2:

Almost the same, but NSEW moves a Waypoint that starts at (10,-1),
LR rotate the Waypoint around origin,
and F adds (Waypoint times value) to the Ship's location

*/

		public record Command
		{
			public char C {get; init;}
			public int Amount {get; init;}

			public Command(char c, int amount) => (C, Amount) = (c, amount);
		}

		Command ParseCommand(string line)
		{
			return new Command(line[0], int.Parse(line[1..]));
		}

		public class Ship
		{
			public int Heading {get; private set;} // 0 = North, 90 = East, etc.
			public int X {get; private set;}
			public int Y {get; private set;}

			public Ship()
			{
				X = Y = 0;
				Heading = 90;

				WaypointX = 10;
				WaypointY = -1;
			}

			public int ManhattanDistanceFromOrigin => Math.Abs(X) + Math.Abs(Y);

			public void Move(Command c)
			{
				switch (c.C)
				{
					case 'N': Y -= c.Amount; break;
					case 'S': Y += c.Amount; break;
					case 'E': X += c.Amount; break;
					case 'W': X -= c.Amount; break;

					case 'L': Heading -= c.Amount; break;
					case 'R': Heading += c.Amount; break;

					case 'F': Move(c with {C = Heading switch {
							0 => 'N',
							90 => 'E',
							180 => 'S',
							270 => 'W',
							_ => throw new Exception($"Unsupported heading: {Heading}")
						}}); break;
				}

				Heading = (360 + Heading) % 360;
			}

			public int WaypointX {get; private set;}
			public int WaypointY {get; private set;}

			public void Move2(Command c)
			{
				switch (c.C)
				{
					case 'N': WaypointY -= c.Amount; break;
					case 'S': WaypointY += c.Amount; break;
					case 'E': WaypointX += c.Amount; break;
					case 'W': WaypointX -= c.Amount; break;

					case 'L': RotateWaypoint(360 - c.Amount); break;
					case 'R': RotateWaypoint(c.Amount); break;

					case 'F':
						foreach (var n in Enumerable.Range(0, c.Amount))
						{
							X += WaypointX;
							Y += WaypointY;
						}
						break;
				}
			}

			private void RotateWaypoint(int degrees)
			{
				foreach (var _ in Enumerable.Range(0, degrees / 90))
					(WaypointX, WaypointY) = (-WaypointY, WaypointX);
			}
		}

		public override object Part1(string input)
		{
			var s = new Ship();

			foreach (var l in input.ToLines())
				s.Move(ParseCommand(l));

			return s.ManhattanDistanceFromOrigin;
		}
		
		public override object Part2(string input)
		{
			var s = new Ship();

			foreach (var l in input.ToLines())
				s.Move2(ParseCommand(l));

			return s.ManhattanDistanceFromOrigin;
		}
	}
}
