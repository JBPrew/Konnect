import csv
with open('Depth Data/KinectDataRaw.csv', 'r', newline='') as file:
    reader = csv.reader(file)
    fileWriter = open("Depth Data/KinectData.csv", "w")
    for row in reader:
        target = row.pop(0) #Remove the target
        if target != "Trash":
            if target == "Side Wall":
                row = list(map(int, row))
                fileWriter.write("Known Obstacle" + ", " + str(row).strip('[]') + "\n")
            elif target == "Unknown Obstacle":
                row = list(map(int, row))
                fileWriter.write("Known Obstacle" + ", " + str(row).strip('[]') + "\n")
            else:
                row = list(map(int, row))
                fileWriter.write(target + ", " + str(row).strip('[]') + "\n")
    fileWriter.close()
        
