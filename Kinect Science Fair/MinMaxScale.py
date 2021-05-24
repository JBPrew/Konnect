import csv

with open('Depth Data/KinectDataDoubleSmoothed.csv', 'r', newline='') as file:
    reader = csv.reader(file)
    fileWriter = open("Depth Data/KinectDataMinMax.csv", "w")
    count = 1
    for row in reader:
        target = row.pop(0) #Remove the target
        row = list(map(int, row))
        maxList = max(row)
        minList = min(i for i in row if i > 0)
        print(maxList)
        print(minList)
        print("\n")
        if count != 1:
            for i in range(len(row)): #Looks down the row
                if (row[i] != 0):
                    row[i] = (row[i] - minList) / (maxList - minList)
        fileWriter.write(target + ", " + str(row).strip('[]') + "\n")
        count+=1
    fileWriter.close()
        
