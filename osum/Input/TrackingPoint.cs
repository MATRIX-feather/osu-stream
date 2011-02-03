using System;
using System.Drawing;
using OpenTK;
namespace osum
{
	public class TrackingPoint
	{
		public object Tag;
		
		private PointF location;
        /// <summary>
        /// The raw screen location 
        /// </summary>
		public PointF Location
		{
			get	{ return location; }
			set { 
				if (location != Point.Empty)
                    Delta = new PointF(value.X - location.X, value.Y - location.Y);
				location = value;
			}
		}
		
		public PointF Delta;


        /// <summary>
        /// Increased for every press that is associated with the tracking point.
        /// </summary>
        int validity;
        
        /// <summary>
        /// Is this point still valid (active)?
        /// </summary>
        public bool Valid { get { return validity > 0; } }
		
		public TrackingPoint(PointF location) : this(location,null)
		{}
			
		public TrackingPoint(PointF location, object tag)
		{
			Location = location;
			Tag = tag;
		}

		public virtual Vector2 WindowPosition
		{
			get
			{
				return new Vector2(GameBase.ScaleFactor * Location.X/GameBase.NativeSize.Width * GameBase.BaseSize.Width, GameBase.ScaleFactor * Location.Y/GameBase.NativeSize.Height * GameBase.BaseSize.Height);	
			}
		}
		
		public virtual Vector2 WindowDelta
		{
			get
			{
				return new Vector2(GameBase.ScaleFactor * Delta.X/GameBase.NativeSize.Width * GameBase.BaseSize.Width, GameBase.ScaleFactor * Delta.Y/GameBase.NativeSize.Height * GameBase.BaseSize.Height);	
			}
		}

        public virtual Vector2 GamefieldPosition
        {
            get
            {
                return GameBase.StandardToGamefield(WindowPosition);
            }
        }



        internal void IncreaseValidity()
        {
            validity++;
        }

        internal void DecreaseValidity()
        {
            validity--;
        }
    }
}

