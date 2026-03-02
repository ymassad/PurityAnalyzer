
# PurityAnalyzer – Conceptual Documentation

## 1. Purpose

PurityAnalyzer is a Roslyn analyzer that allows developers to declare purity contracts in C# and have them statically validated.

Purity means:

> The method does not depend on hidden state and does not modify shared state.

The goal is not to ban mutation.  
The goal is to make side effects explicit and controlled.

---

## 2. What “Pure” Means in This System

A method marked `[IsPure]` must:

- Not read static fields  
- Not write static fields  
- Not read instance fields  
- Not write instance fields  
- Not call impure methods  
- Not rely on hidden global behavior  

It may:

- Read its parameters  
- Mutate objects it freshly creates  
- Return newly created objects  

### Example

```csharp
[IsPure]
List<int> Create()
{
    var list = new List<int>();
    list.Add(1);
    return list;
}
```

This is pure because the list is created inside the method and is not shared.

Mutation of fresh objects is allowed.  
Mutation of shared objects is not.

---

## 3. Controlled Mutation

C# allows mutation for performance and expressiveness.  
PurityAnalyzer allows mutation — but in a controlled way.

### 3.1 `[IsPureExceptLocally]`

This allows mutation of state owned by the object.

Use this when:

- You want internal mutation for speed.  
- The mutation does not escape.  
- The mutated state is fully owned.  

Example idea:

- Internal buffers  
- Internal counters  
- Internal data structures  

**Rule:**

> You may mutate state that belongs only to this object and is not externally reachable.

---

### 3.2 `[IsPureExceptReadLocally]`

`[IsPureExceptReadLocally]` allows a method to read its own instance fields, but not modify them.

The important usefulness is this:

> A method marked `[IsPure]` is allowed to call a method marked `[IsPureExceptReadLocally]` on one of its parameters.

Why?

Because reading the internal state of a parameter is not considered impurity.  
The pure method is still only depending on its inputs.

---

## 4. Fresh vs Shared Objects

PurityAnalyzer makes a strong distinction:

- Fresh object → safe to mutate  
- Shared object → unsafe to mutate  

Fresh means:

- Created inside the method  
- Not previously exposed  
- Not reachable from outside  

If you mutate a shared object, the method is impure.  
If you mutate a fresh object before returning it, the method can still be pure.

---

## 5. PureLambdaClass.Pure

Lambdas cannot have attributes.

So `PureLambdaClass.Pure` exists to say:

> “Validate this lambda as pure.”

It works like `[IsPure]` but for lambdas.

It checks that the lambda:

- Does not access static state  
- Does not mutate captured outer variables  
- Does not read parent locals  
- Does not call impure methods  

It is simply a validation anchor.  
It does not create purity.  
It verifies purity.

We should not claim that the entire system depends on it for correctness.  
It is a way to validate lambdas explicitly when needed.

---

## 6. Potentially Pure Functions

Some functions are pure except that they call functions passed to them.

### Example

```csharp
static T Compute<T>(Func<T> f) => f();
```

Whether `Compute` is pure depends on `f`.

PurityAnalyzer handles this by analyzing the lambda passed to such methods.  
If the lambda is impure, the calling method becomes impure.

So PurityAnalyzer assumes potentially-pure methods to be pure as far as the analysis of the method itself is concerned. Callers passing impure functions are considered to be impure.

---

## 7. Casting and Purity

Assume:

```csharp
Class1 x = ...;
BaseClass1 b = (BaseClass1)x;
b.Method1();
```

After the cast, at the call site `b.Method1()`, the analyzer reasons using `BaseClass1.Method1` (the method visible on the static type), not `Class1.Method1`.

So the risk is:

> `Class1.Method1` could be less pure than what `BaseClass1.Method1` promises.

Because at runtime, `b.Method1()` will dispatch to `Class1.Method1` (override/implementation). If `BaseClass1.Method1` is treated as “pure enough” but the actual `Class1.Method1` is less pure, purity can be incorrectly accepted.

Therefore the rule is:

> When casting from `Class1` to `BaseClass1`, PurityAnalyzer checks that each implementation in `Class1` is at least as pure as the corresponding method contract on `BaseClass1`.

In other words:

- Base type sets the expectation.  
- Concrete type must not be weaker.  
- The cast is the place where the analyzer enforces that this expectation is safe to rely on later.

---

## 8. Generic Type Parameters and NotUsedAsObject

For a type parameter `T`, operations like these:

- Calling `ToString()`, `Equals(...)`, `GetHashCode()`  
- Casting `T` to `object`  
- Using `T` in a way that forces object-level behavior  

are effectively “treating `T` as object”.

That matters because:

- `object` methods are virtual  
- Runtime types can override them  
- Overrides can be impure  

So if `T` is used as object, the analyzer must apply the same strict rules it applies to casts and dynamic dispatch.

### Why NotUsedAsObject is needed

Without `NotUsedAsObject`, the analyzer must conservatively assume:

> This type parameter might be used as object.

And if that assumption is true, the analyzer must reject purity unless it can prove the relevant object-level behavior is safe.

`NotUsedAsObject` tells the analyzer:

> This type parameter is never treated as object.

So PurityAnalyzer does not need to worry about `ToString`, `Equals`, `GetHashCode`, or other object-level virtual dispatch risks for that `T`.

In short:

- If `T` is used as object → object/cast rules apply → purity may be rejected.  
- If `T` is not used as object (`NotUsedAsObject`) → those object/cast concerns don’t apply → purity can be accepted if the rest is pure.

---

## 9. Framework Method Modeling

PurityAnalyzer includes built-in lists of known framework methods that are treated as:

- Pure  
- PureExceptReadLocally  
- PureExceptLocally  
- Returning new objects  

This is necessary because:

- The analyzer cannot analyze the entire .NET framework source.  
- Some framework methods are known to be safe.  
- Others are not.  

Example:

`int.ToString()` depends on culture.  
So purity must consider that.

---

## 10. object.GetHashCode Special Handling

Consider:

```csharp
[IsPure]
int F()
{
    return new object().GetHashCode();
}
```

This method should be impure.

Why?

Because the default implementation of `object.GetHashCode()` in .NET is identity-based.  
Each new object instance will typically produce a different hash code.

So:

```csharp
new object().GetHashCode()
```

does not return a value derived from inputs.  
It returns a value derived from object identity.

That makes the method effectively impure.

### Why normal rules are not enough

To mark this method as impure using existing rules, we would have to either:

1. Treat `new object()` as impure  
2. Treat `object.GetHashCode()` as impure  

Both are unacceptable:

- Object construction is fundamental and cannot be considered impure.  
- `GetHashCode()` is widely used and cannot be globally marked impure.

### The solution

PurityAnalyzer treats `object.GetHashCode()` as a special purity category.

It is not considered generally pure or generally impure.  
It is tracked separately so that:

- `new object().GetHashCode()` is correctly treated as impure.  
- Object construction remains valid.  
- Normal `GetHashCode` usage does not break the analyzer.
