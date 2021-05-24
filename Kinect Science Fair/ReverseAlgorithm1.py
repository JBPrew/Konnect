import csv

with open('Depth Data/KinectDataRaw.csv', 'r', newline='') as file:
    reader = csv.reader(file)
    fileWriter = open("Depth Data/KinectDataCorrect.csv", "w")
    index = 0

    reverseSpot = 487 #CHANGE
    for row in reader:
        target = row.pop(0) #Remove the target
        row = list(map(int, row))
        index+=1
        if index >= reverseSpot:
            row.reverse()
        fileWriter.write(target + ", " + str(row).strip('[]') + "\n")
    fileWriter.close()