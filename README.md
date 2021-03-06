# MultivariateTestProvider
F# type provider for statically typed test variants

Given a text file with the format:

    Test1
    	VariantA: 23
    	VariantB: 67
    	VariantC: 10
    Test2
    	VariantA: 10
    	VariantB: 90

It will allow you to get statically typed access to the test variant names and their weights:

![Statically typed access to test variants](/imgs/staticallytypedvariants.png)

If you provide a config file that includes variants totalling more than 100%, e.g.

    Test1
    	VariantA: 23
    	VariantB: 67
    	VariantC: 10
    	VariantD: 10
    Test2
    	VariantA: 10
    	VariantB: 90

You will get an error message and it won't compile: 

![Variants totalling more than 100%](/imgs/morethan100percent.png)

This is similar to adding a config file with tests or variants with the same name:

    Test1
    	VariantA: 23
    	VariantB: 67
    	VariantC: 10
    Test2
    	VariantA: 10
    	VariantA: 90

Results in:

![Duplicate variants error message](/imgs/duplicatevariants.png)
