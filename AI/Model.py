import torch.nn as nn
import torch

class Model(nn.Module):
    def __init__(self, device, additional_layers=[]):
        super().__init__()
        self.device = device

        self.linear_pipeline = nn.Sequential(
            nn.LazyLinear(256),
            nn.Tanh(),
            nn.LazyLinear(128),
            nn.Tanh(),
        )
        self.pixel_pipeline = nn.Sequential(
            nn.LazyConv2d(16, 5),
            nn.ReLU(),
            nn.LazyConv2d(8, 4),
            nn.ReLU(),
            nn.MaxPool2d(3),
            nn.Flatten(-3, -1),
        )

        self.combined_pipeline = nn.Sequential(
            nn.LazyLinear(128),
            nn.Tanh(),
            nn.LazyLinear(64),
            nn.Tanh(),
            *additional_layers
        )
    

    def forward(self, pixel_data, linear_data):
        if (pixel_data.device.type != self.device.type):
            pixel_data = pixel_data.to(self.device)
            linear_data = linear_data.to(self.device)

        image_transformed = self.pixel_pipeline(pixel_data)
        linear_transformed = self.linear_pipeline(linear_data)
        combined = torch.concat((image_transformed, linear_transformed), dim=-1)
        combined_transformed = self.combined_pipeline(combined)
        return combined_transformed