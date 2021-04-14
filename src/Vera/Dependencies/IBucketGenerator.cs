namespace Vera.Dependencies
{
    public interface IBucketGenerator<T>
    {
        /// <summary>
        /// Generates a string that is the same for every entity that would
        /// belong to the same 'bucket'. E.g. x and z reports have their own buckets.
        /// </summary>
        /// <param name="enntity">The entity to generate the bucket for</param>
        /// <returns>The generated 'bucket' string</returns>
        string Generate(T enntity);
    }
}
