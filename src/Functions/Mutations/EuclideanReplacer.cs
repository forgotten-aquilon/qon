using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using qon.Exceptions;
using qon.Functions.Filters;
using qon.Helpers;
using qon.Layers.StateLayers;
using qon.Machines;
using qon.Variables;

namespace qon.Functions.Mutations
{
    public class EuclideanReplacer<TQ> : IMutationFunction<TQ>  where TQ : notnull
    {
        private readonly struct Dimension
        {
            public readonly int X, Y, Z;

            public Dimension(int x, int y, int z)
            {
                X = x;
                Y = y; 
                Z = z;
            }
        }

        private readonly (char a1, char a2, char a3)[] _axisPermutations =
        {
            ('X', 'Y', 'Z'),
            ('X', 'Z', 'Y'),
            ('Y', 'X', 'Z'),
            ('Y', 'Z', 'X'),
            ('Z', 'X', 'Y'),
            ('Z', 'Y', 'X')
        };

        private readonly QPredicate<TQ>[,,] _pattern;
        private readonly VariableMutation<TQ>[,,] _mutation;
        private readonly Dimension _dimension;
        private Dimension _fieldDimension;

        public EuclideanReplacer(QPredicate<TQ>[,,] pattern, VariableMutation<TQ>[,,] mutation)
        {
            _pattern = pattern;
            _mutation = mutation;

            int d1 = _pattern.GetLength(0);
            int d2 = _pattern.GetLength(1);
            int d3 = _pattern.GetLength(2);

            if (d1 != _mutation.GetLength(0) || d2 != _mutation.GetLength(1) || d3 != _mutation.GetLength(2))
            {
                throw new InternalLogicException("");
            }

            _dimension = new Dimension(d3, d2, d1);
        }

        public List<Field<TQ>> ApplyTo(Field<TQ> input)
        {
            var layer = EuclideanStateLayer<TQ>.With(input.Machine.State);

            var grid = layer.FieldGrid;

            var cols = grid.GetLength(0);
            var rows = grid.GetLength(1);
            var dpth = grid.GetLength(2);

            _fieldDimension = new Dimension(rows, cols, dpth);

            var anchorPattern = _pattern[0, 0, 0];

            List<(int x, int y, int z)> anchors = new List<(int, int, int)>();

            int linearCoord = 0;

            for (int i = 0; i < dpth; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    for (int k = 0; k < cols; k++)
                    {
                        if (anchorPattern.ApplyTo(input[linearCoord]))
                        {
                            anchors.Add((k, j, i));
                        }

                        linearCoord++;
                    }
                }
            }

            List<Field<TQ>> result = new List<Field<TQ>>();


            foreach (var anchor in anchors)
            {
                foreach (var permutation in _axisPermutations)
                {
                    if (_dimension.Y == _dimension.Z && permutation is ('X', 'Z', 'Y') or ('Z', 'X', 'Y') or ('Z', 'Y', 'X'))
                    {
                        continue;
                    }

                    if (_dimension.X == _dimension.Z && permutation is ('Z', 'Y', 'X') or ('Z', 'X', 'Y') or ('Y', 'Z', 'X'))
                    {
                        continue;
                    }

                    if (_dimension.Y == _dimension.X && permutation is ('Y', 'X', 'Z') or ('Y', 'Z', 'X') or ('Z', 'Y', 'X'))
                    {
                        continue;
                    }

                    for (byte i = 0; i <= 7; i++)
                    {
                        bool bx = (i & 0b00000_001) != 0;
                        bool by = (i & 0b00000_010) != 0;
                        bool bz = (i & 0b00000_100) != 0;

                        if (bx && _dimension.X == 1)
                        {
                            continue;
                        }

                        if (by && _dimension.Y == 1)
                        {
                            continue;
                        }

                        if (bz && _dimension.Z == 1)
                        {
                            continue;
                        }

                        var reverse = (x: bx, y: by, z: bz);

                        if (!FitsInField(anchor, permutation, reverse))
                            continue;


                        if (MutateField(input, anchor, permutation, reverse) is {} field)
                        {
                            result.Add(field);
                        }
                    }
                }
            }

            return result;
        }

        private Field<TQ>? MutateField(Field<TQ> field, (int x, int y, int z) anchor, (char a1, char a2, char a3) axis, (bool x, bool y, bool z) reverse)
        {
            var newField = field.ShallowCopy();

            for (int i = 0; i < _dimension.Z; i++)
            {
                for (int j = 0; j < _dimension.Y; j++)
                {
                    for (int k = 0; k < _dimension.X; k++)
                    {
                        int newK = GetAxis(k, i, j, axis.a1);
                        int newJ = GetAxis(k, i, j, axis.a2);
                        int newI = GetAxis(k, i, j, axis.a3);

                        bool newReverseX = GetAxis(reverse.x, reverse.y, reverse.z, axis.a1);
                        bool newReverseY = GetAxis(reverse.x, reverse.y, reverse.z, axis.a2);
                        bool newReverseZ = GetAxis(reverse.x, reverse.y, reverse.z, axis.a3);

                        int newX = !newReverseX ? anchor.x + newK : anchor.x - newK;
                        int newY = !newReverseY ? anchor.y + newJ : anchor.y - newJ;
                        int newZ = !newReverseZ ? anchor.z + newI : anchor.z - newI;

                        var cell = field[ToLinearCoord((newX, newY, newZ), (_fieldDimension.X, _fieldDimension.Y, _fieldDimension.Z))];

                        if (_pattern[i, j, k].ApplyTo(cell))
                        {
                            var newCell = cell.Copy();
                            newField[cell.Id] = newCell;
                            _mutation[i, j, k].Execute(newCell);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

            return newField;
        }

        private bool FitsInField(
            (int x, int y, int z) anchor,
            (char a1, char a2, char a3) axis,
            (bool x, bool y, bool z) reverse)
        {
            int newXLimit = GetAxis(_dimension.X, _dimension.Y, _dimension.Z, axis.a1) - 1;
            int newYLimit = GetAxis(_dimension.X, _dimension.Y, _dimension.Z, axis.a2) - 1;
            int newZLimit = GetAxis(_dimension.X, _dimension.Y, _dimension.Z, axis.a3) - 1;

            bool newReverseX = GetAxis(reverse.x, reverse.y, reverse.z, axis.a1);
            bool newReverseY = GetAxis(reverse.x, reverse.y, reverse.z, axis.a2);
            bool newReverseZ = GetAxis(reverse.x, reverse.y, reverse.z, axis.a3);

            int newX = !newReverseX ? anchor.x + newXLimit : anchor.x - newXLimit;
            int newY = !newReverseY ? anchor.y + newYLimit : anchor.y - newYLimit;
            int newZ = !newReverseZ ? anchor.z + newZLimit : anchor.z - newZLimit;

            if (newX >= _fieldDimension.X || newX < 0 || newY >= _fieldDimension.Y || newY < 0 || newZ >= _fieldDimension.Z || newZ < 0)
            {
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ToLinearCoord((int x, int y, int z) coord, (int x, int y, int z) dimensions)
        {
            return ((coord.z * dimensions.y + coord.y) * dimensions.x + coord.x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T GetAxis<T>(T a1, T a2, T a3, char a)
        {
            return a switch
            {
                'X' => a1,
                'Y' => a2,
                'Z' => a3,
                _ => throw new NonExhaustiveExpressionException(a)
            };
        }

        public static QPredicate<TQ>[,,] CreatePatternFrom(IEnumerable<TQ> linearPattern)
        {
            var arr = linearPattern.Select(x => (QPredicate<TQ>)x).ToArray();

            var result = new QPredicate<TQ>[1, 1, arr.Length];

            for (int i = 0; i < arr.Length; i++)
            {
                result[0, 0, i] = arr[i];
            }

            return result;
        }

        public static QPredicate<TQ>[,,] CreatePatternFrom(params TQ?[] linearPattern)
        {
            var arr = linearPattern.Select(x => x is not null ? x : new QPredicate<TQ>(v => true)).ToArray();

            var result = new QPredicate<TQ>[1, 1, arr.Length];

            for (int i = 0; i < arr.Length; i++)
            {
                result[0, 0, i] = arr[i];
            }

            return result;
        }

        public static VariableMutation<TQ>[,,] CreateMutationFrom(IEnumerable<TQ> linearMutation)
        {
            var arr = linearMutation.Select(VariableMutation<TQ>.FromValue).ToArray();

            var result = new VariableMutation<TQ>[1, 1, arr.Length];

            for (int i = 0; i < arr.Length; i++)
            {
                result[0, 0, i] = arr[i];
            }

            return result;
        }

        public static VariableMutation<TQ>[,,] CreateMutationFrom(params TQ?[] linearMutation)
        {
            var arr = linearMutation.Select(x => x is not null ? VariableMutation<TQ>.FromValue(x) : VariableMutation<TQ>.Empty).ToArray();

            var result = new VariableMutation<TQ>[1, 1, arr.Length];

            for (int i = 0; i < arr.Length; i++)
            {
                result[0, 0, i] = arr[i];
            }

            return result;
        }
    }
}
