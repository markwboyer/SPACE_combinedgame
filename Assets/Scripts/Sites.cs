namespace Sites {
    public class LandingSite
	{
		public LandingSite(int I, int J, float Diff, float Alt)
		{
			this.i = I;
			this.j = J;
			this.diff = Diff;
			this.alt = Alt;
		}
		public int i { set; get; } //landing site i coordinate
		public int j { set; get; } //landing site j coordinate
		public float diff { set; get; } //landing site difference in slope
		public float alt { set; get; } //landing site altitude
	}

	public class ScienceSite
	{
		public ScienceSite(int I, int J)
		{
			this.i = I;
			this.j = J;
		}
		public int i { set; get; } //science site i coordinate
		public int j { set; get; } //science site j coordinate

	}

	public class Rock
	{
		public Rock(int I, int J)
		{
			this.i = I;
			this.j = J;
		}
		public int i { set; get; } //rock i coordinate
		public int j { set; get; } //rock j coordinate

	}
}