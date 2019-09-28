using System;
using System.Collections;
using System.Collections.Generic;

namespace ScintillaNET.WPF.Configuration
{
	public sealed class ScintillaWPFConfigItemCollection : IList, ICollection<ScintillaWPFConfigItem>
	{
		private readonly ScintillaWPF mParentScintilla;
		internal ScintillaWPFConfigItemCollection(ScintillaWPF parent)
		{
			this.mParentScintilla = parent;
		}

		private readonly List<ScintillaWPFConfigItem> mConfigItems = new List<ScintillaWPFConfigItem>();
		private readonly HashSet<Type> mAddedConfigTypes = new HashSet<Type>();
		public int Add(object value)
		{
			if (!(value is ScintillaWPFConfigItem))
				throw new NotImplementedException();
			var t = value.GetType();
			if (mAddedConfigTypes.Contains(t))
				throw new Exception("Attempted to define the same config item multiple times!");
			mAddedConfigTypes.Add(t);
			mConfigItems.Add((ScintillaWPFConfigItem)value);
			((ScintillaWPFConfigItem)value).ApplyConfig(mParentScintilla);
			return 0;
		}
		public void Clear()
		{
			foreach (var c in mConfigItems)
				c.Reset(mParentScintilla);
			mConfigItems.Clear();
			mAddedConfigTypes.Clear();
		}
		public bool IsFixedSize { get { return false; } }
		public bool IsReadOnly { get { return false; } }
		public int Count { get { return mConfigItems.Count; } }
		public bool IsSynchronized { get { return false; } }
		public object SyncRoot { get { return null; } }
		public IEnumerator GetEnumerator() { return ((IList)mConfigItems).GetEnumerator(); }
		public bool Contains(object value) { return ((IList)mConfigItems).Contains(value); }
		public int IndexOf(object value) { return ((IList)mConfigItems).IndexOf(value); }
		public void Remove(object value)
		{
			var c = (ScintillaWPFConfigItem)value;
			c.Reset(mParentScintilla);
			mAddedConfigTypes.Remove(c.GetType());
			((IList)mConfigItems).Remove(value);
		}
		public void RemoveAt(int index)
		{
			var c = mConfigItems[index];
			c.Reset(mParentScintilla);
			mAddedConfigTypes.Remove(c.GetType());
			((IList)mConfigItems).RemoveAt(index);
		}
		public void Insert(int index, object value)
		{
			if (mAddedConfigTypes.Contains(value.GetType()))
				throw new Exception("Attempted to define the same config item multiple times!");
			var c = (ScintillaWPFConfigItem)value;
			c.ApplyConfig(mParentScintilla);
			mAddedConfigTypes.Add(value.GetType());
			((IList)mConfigItems).Insert(index, value);
		}
		public ScintillaWPFConfigItem this[int index]
		{
			get { throw new NotImplementedException("Item_get"); }
			set { throw new NotImplementedException("Item_set"); }
		}
		object IList.this[int index]
		{
			get { return mConfigItems[index]; }
			set
			{
				if (!(value is ScintillaWPFConfigItem))
					throw new NotImplementedException();
				mConfigItems[index].Reset(mParentScintilla);
				mAddedConfigTypes.Remove(mConfigItems[index].GetType());
				if (mAddedConfigTypes.Contains(value.GetType()))
					throw new Exception("Attempted to define the same config item multiple times!");
				mAddedConfigTypes.Add(value.GetType());
				mConfigItems[index] = (ScintillaWPFConfigItem)value;
				mConfigItems[index].ApplyConfig(mParentScintilla);
			}
		}
		public void CopyTo(Array array, int index) { throw new NotImplementedException(); }
		public void CopyTo(ScintillaWPFConfigItem[] array, int arrayIndex) { throw new NotImplementedException(); }

		public void Add(ScintillaWPFConfigItem item) { this.Add((object)item); }
		public bool Contains(ScintillaWPFConfigItem item) { return this.Contains((object)item); }

		public bool Remove(ScintillaWPFConfigItem item) { this.Remove((object)item); return true; }
		IEnumerator<ScintillaWPFConfigItem> IEnumerable<ScintillaWPFConfigItem>.GetEnumerator() { return mConfigItems.GetEnumerator(); }
	}
}
