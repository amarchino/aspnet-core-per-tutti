class Apple {
  public string Color { get; set; }
  public int Weight { get; set; }
}

List<Apple> apples = new List<Apple> {
  new Apple { Color = "Red", Weight = 180 },
  new Apple { Color = "Green", Weight = 195 },
  new Apple { Color = "Red", Weight = 190 },
  new Apple { Color = "Green", Weight = 185 }
};

IEnumerable<Apple> redApples = apples.Where(apple => apple.Color == "Red");
Console.WriteLine(apples);
Console.WriteLine(redApples);

double averageWeight = apples
  .Where(apple => apple.Color == "Red")
  .Select(apple => apple.Weight)
  .Average();

Console.WriteLine($"Average weight: {averageWeight}");

apples
  .Where(apple => apple.Color == "Red")
  .Select(apple => apple.Weight)
  .ToList()
  .ForEach(w => Console.WriteLine(w));
