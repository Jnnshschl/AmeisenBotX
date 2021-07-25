using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Memory.Structs
{
    public class AllocationPool
    {
        public AllocationPool(IntPtr address, int size)
        {
            Address = address;
            Size = size;

            Allocations = new();
        }

        public IntPtr Address { get; }

        public SortedList<int, int> Allocations { get; }

        public int Size { get; }

        /// <summary>
        /// Free a memory block in the pool
        /// </summary>
        /// <param name="address">Allocation address</param>
        /// <returns>True, when the block has been found and freed, false if not</returns>
        public bool Free(IntPtr address)
        {
            int addressInt = address.ToInt32();
            int baseAddressInt = Address.ToInt32();

            if (addressInt >= baseAddressInt
                && addressInt < baseAddressInt + Size)
            {
                Allocations.Remove(addressInt - baseAddressInt);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to reserve a memory block in the pool
        /// </summary>
        /// <param name="size">Size of the wanted block</param>
        /// <param name="address">Address of the memory allocation</param>
        /// <returns>True when a block could be reserved, false if not</returns>
        public bool Reserve(int size, out IntPtr address)
        {
            if (GetNextFreeBlock(size, out int offset))
            {
                Allocations.Add(offset, size);
                address = IntPtr.Add(Address, offset);
                return true;
            }

            address = IntPtr.Zero;
            return false;
        }

        private bool GetNextFreeBlock(int size, out int offset)
        {
            if (size <= Size)
            {
                if (Allocations.Count == 0)
                {
                    offset = 0;
                    return true;
                }
                else
                {
                    for (int i = 0; i < Allocations.Count; ++i)
                    {
                        KeyValuePair<int, int> allocation = Allocations.ElementAt(i);
                        int allocationEnd = allocation.Key + allocation.Value;

                        // when there is a next element, used it as the limiter, if not use the whole remaining space
                        int memoryLeft = i + 1 < Allocations.Count 
                            ? Allocations.ElementAt(i + 1).Key - allocationEnd 
                            : Size - allocationEnd;

                        if (memoryLeft >= size)
                        {
                            offset = allocationEnd;
                            return true;
                        }
                    }
                }
            }

            offset = 0;
            return false;
        }
    }
}