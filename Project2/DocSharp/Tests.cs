#define TEST

// This namespace is used for testing the documentation of behavior not present in the DocSharp code.
namespace DocSharp.Tests {
    /// Test generics
    /// T1 = Type parameter summary
    /// T2 = Other type parameter summary
    public class GenericTestClass<T1, T2> { }

    /// Test abstract classes
    public abstract class AbstractTestClass {
#if RELEASE  &&  (TEST || UNDEFINED) && true // Tests the preprocessor instruction parser
        public class EmbeddedClass { public const string Hello = "Hello, \"World\"!"; }
#endif
        readonly GenericTestClass<string, int> GenericTestField = new GenericTestClass<string, int>();
        /// Test abstract functions
        public abstract void AbstractTestFunction();

        /// Test generic functions.
        /// x = parameter summary
        public GenericTestClass<string, int> TestFunction(int x) { return GenericTestField; }
    }
}