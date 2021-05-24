import csv

def nearestNeighborBefore(i):
    for j in range(1, 6):
        if (((i-j) >= 0) and (row[i - j] != 0) and not ((i-j) in wasZero)):
            return j, row[i-j]
            break
    return 7, 0

def nearestNeighborAfter(i):
    for k in range(1, 6):
        if ((i+k <= len(row) -1) and row[i + k] != 0):
            return k, row[i+k]
            break
    return 7, 0

with open('Depth Data/KinectDataWallConfig.csv', 'r', newline='') as file:
    reader = csv.reader(file)
    fileWriter = open("Depth Data/KinectDataDoubleSmoothed.csv", "w")
    for row in reader:
        target = row.pop(0) #Remove the target
        row = list(map(int, row))
        wasZero = []
        for i in range(len(row)): #Looks down the row
            if (row[i] == 0):
                wasZero.append(i)
                neighborBeforeIndex, neighborBeforeValue = nearestNeighborBefore(i)
                neighborAfterIndex, neighborAfterValue = nearestNeighborAfter(i)
                threshold = 6 # Max distance between two values' indices
                if (neighborBeforeIndex + neighborAfterIndex <= threshold and abs(neighborBeforeValue - neighborAfterValue) <= 20):
                    row[i] = int((neighborBeforeValue + neighborAfterValue)/2) #Sets row as average
        fileWriter.write(target + ", " + str(row).strip('[]') + "\n")
    fileWriter.close()
        
